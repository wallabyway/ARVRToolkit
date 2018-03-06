//
// Copyright (c) Autodesk, Inc. All rights reserved.
// 
// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autodesk.Forge.ARKit {

	// RenderScene.js #467
	public class ExplodeRuntime {

		#region Enums
		public enum FragmentListEnum {
			MESH_VISIBLE = 0x01,
			MESH_HIGHLIGHTED = 0x02,
			MESH_HIDE = 0x04,
			MESH_ISLINE = 0x08,
			MESH_MOVED = 0x10, // indicates if an animation matrix is set
			MESH_TRAVERSED = 0x20, // only used for paging: drawn fragments are tagged and then skipped by forEach() until the flag is being reset (e.g. on scene/camera changes)
			MESH_DRAWN = 0x40, // only used for paging: drawn fragments are tagged. At the end of all render passes flag is copied to MESH_TRAVERSED.
			MESH_RENDERFLAG = 0x80
		};

		#endregion

		#region Structures
		public class StoreTransform {
			public Vector3 localPosition;
			public Quaternion localRotation;
			public Vector3 localScale;
		}

		#endregion

		#region Fields
		public GameObject _root = null;
		public float [] _animxforms = null; // If animation is used, this is a Float32Array storing 10 floats per fragment to describe scale (3), rotation (4), and translation (3).
		public byte [] _vizflags = null;
		private int _explodeDepth = 0;
		//private int _scaledExplodeDepth =0 ;
		private int _currentSegmentFraction = 0;
		protected List<Bounds> _boxes0 = null;
		protected Dictionary<string, StoreTransform> _trs0 = null;
		protected Vector3 _mc0 = Vector3.zero;
		public float _scale = 1.0f;

		#endregion

		#region Constructors
		public ExplodeRuntime (GameObject root) {
			_root = root;
			MeshFilter [] filters = _root.GetComponentsInChildren<MeshFilter> ();
			int initialSize = filters.Length;
			if ( initialSize <= 0 )
				initialSize = 1;
			_vizflags = new byte [initialSize]; // visibility/highlight mode flags
			_boxes0 = GameObjectsBounds (_root);
			List<GameObject> list = GameObjectsList (_root);
			_trs0 = new Dictionary<string, StoreTransform> ();
			foreach ( GameObject obj in list ) {
				StoreTransform tr = new StoreTransform ();
				tr.localPosition = obj.transform.localPosition;
				tr.localRotation = obj.transform.localRotation;
				tr.localScale = obj.transform.localScale;
				_trs0 [obj.name] = tr;
			}
			_mc0 = GameObjectBounds (_root).center;
		}

		#endregion

		#region Bounding Box
		public static Bounds GameObjectBounds (GameObject obj) {
			Renderer [] renderers = obj.GetComponentsInChildren<Renderer> ();
			if ( renderers.Length == 0 )
				return (new Bounds ());
			Bounds bounds = new Bounds (renderers [0].bounds.center, renderers [0].bounds.size);
			foreach ( Renderer renderer in renderers )
				bounds.Encapsulate (renderer.bounds);
			return (bounds);
		}

		public static List<Bounds> GameObjectsBounds (GameObject obj) {
			List<Bounds> list = new List<Bounds> ();
			Renderer [] renderers = obj.GetComponentsInChildren<Renderer> ();
			if ( renderers.Length == 0 )
				return (list);
			Bounds bounds = new Bounds (renderers [0].bounds.center, renderers [0].bounds.size);
			foreach ( Renderer renderer in renderers ) {
				list.Add (renderer.bounds);
				bounds.Encapsulate (renderer.bounds);
			}
			return (list);
		}

		public static List<GameObject> GameObjectsList (GameObject obj) {
			List<GameObject> list = new List<GameObject> ();
			Renderer [] renderers = obj.GetComponentsInChildren<Renderer> ();
			foreach ( Renderer renderer in renderers )
				list.Add (renderer.gameObject);
			return (list);
		}

		#endregion

		#region Explode Methods
		// Define recursive function to traverse object hierarchy. Each object is shifted away 
		// from the bbox center of its parent.
		//  number nodeId:   dbId of the current instanceTree node
		//  int depth:       tracks hierarchy level (0 for root)
		//  vec3 (cx,cy,cz): center of the parent object (after applying the displacement to the parent object) 
		//  vec3 (ox,oy,oz): accumuled displacement from all parents on the path to root
		protected void explodeRec (float scale, GameObject nodeId, int depth, Vector3 parentCenter, Vector3 accumulatedDisplacement) {
			var oscale = scale * 2; // TODO: also possibly related to depth
			if ( depth == _explodeDepth )
				oscale *= _currentSegmentFraction; // smooth transition of this tree depth from non-exploded to exploded state

			// get bbox center of this node
			Bounds tmpBox = GameObjectBounds (nodeId);
			Vector3 mycx = tmpBox.center;

			// The root node (depth==0) has no parent to shift away from.
			// For child nodes with level > explodDepth, we don't apply additional displacement anymore - just pass the displacement of the parents.
			if ( depth > 0 && depth <= _explodeDepth ) {
				// add displacement to move this object away from its parent's bbox center (cx, cy, cz)
				Vector3 dx = (mycx - parentCenter) * oscale;
				dx.z = (mycx.z - parentCenter.z) * oscale;
				//var omax =Math.max (dx.x, Math.max (dx.y, dx.z)) ;
				// sum up offsets: The final displacement of a node is accumulated by its own shift and 
				// the shifts of all nodes up to the root.
				accumulatedDisplacement += dx;
			}

			// continue recursion with child objects (if any)
			for ( int i = 0 ; i < nodeId.transform.childCount ; i++ ) {
				GameObject dbId = nodeId.transform.GetChild (i).gameObject;
				explodeRec (scale, dbId, depth + 1, mycx, accumulatedDisplacement);
			}

			Vector3 pt = accumulatedDisplacement;
			// set translation as anim transform for all fragments associated with the current node
			MeshFilter [] filters = nodeId.GetComponentsInChildren<MeshFilter> ();
			foreach ( MeshFilter filter in filters ) {
				int dbId = 0, fragId = 0;
				string pathid = "";
				RequestObjectInterface.decodeName (filter.gameObject.name, ref dbId, ref fragId, ref pathid);
				updateAnimTransformPos (fragId, ref pt);
			}
		}

		// Sets animation transforms for all fragments to create an "exploded view": Each fragment is displaced  
		// away from the model bbox center, so that you can distuinguish separate components. 
		//
		// If the model data provides a model hierarchy (given via model.getData().instanceTree), it is also considered for the displacement.
		// In this case, we recursively shift each object away from the center of its parent node's bbox. 
		//
		// @param {number} scale - In [0,1]. 0 means no displacement (= reset animation transforms). 
		//                                   1 means maximum displacement, where the shift distance of an object varies 
		//                                   depending on distance to model center and hierarchy level.
		public void explode (float scale) {
			if ( _root == null )
				return;
			_scale = scale;

			Vector3 pt = new Vector3 ();
			//var it =model.getData ().instanceTree ;
			//cyr		MeshFilter[] filters =_root.GetComponentsInChildren<MeshFilter> () ;
			Vector3 mc = _mc0; //GameObjectBounds (_root).center ;

			// Input scale is in the range 0-1, where 0
			// means no displacement, and 1 maximum reasonable displacement.
			scale *= 2;

			// If we have a full part hierarchy we can use a
			// better grouping strategy when exploding
			//if ( it && it.nodeAccess.nodeBoxes && scale != 0 ) {
			//	// If scale is small (close to 0), the shift is only applied to the topmost levels of the hierarchy.
			//	// With increasing s, we involve more and more hierarchy levels, i.e., children are recursively shifted 
			//	// away from their parent node centers.
			//	// Since explodeValue is integer, it will behave discontinous during a transition from s=0 to s=1.
			//	// To keep the overall transition continuous, we use the fractional part of scaledExplodeDepth
			//	// to smoothly fade-in the transition at each hierarchy level. 

			//	// levels beyond explodeDepth, we stop shifting children away from their parent.
			//	_scaledExplodeDepth =scale * (it.maxDepth - 1) + 1 ;
			//	_explodeDepth =0 | _scaledExplodeDepth ;
			//	_currentSegmentFraction =_scaledExplodeDepth - _explodeDepth ;

			//	// Call recursive function to traverse object hierarchy. Each object is shifted away 
			//	// from the bbox center of its parent.
			//	//  number nodeId:   dbId of the current instanceTree node
			//	//  int depth:       tracks hierarchy level (0 for root)
			//	//  vec3 (cx,cy,cz): center of the parent object (after applying the displacement to the parent object) 
			//	//  vec3 (ox,oy,oz): accumuled displacement from all parents on the path to root
			//	explodeRec (scale, it.getRootId (), 0, mc, Vector3.zero) ; // run on root to start recursion
			//} else {
			List<GameObject> gos = GameObjectsList (_root);
			for ( int i = 0 ; i < _boxes0.Count ; i++ ) {
				Transform tr = gos [i].transform;
				StoreTransform tr0 = _trs0 [gos [i].name];
				if ( scale == 0 ) {
					// reset to unexploded state, i.e., remove all animation transforms
					updateAnimTransformFrag (i);
					tr.localPosition = tr0.localPosition;
					tr.localRotation = tr0.localRotation;
					tr.localScale = tr0.localScale;
				} else {
					// get start index of the bbox for fragment i. 
					Bounds bounds = _boxes0 [i];
					// get bbox center of fragment i
					Vector3 cx = bounds.center;
					// compute translation vector for this fragment:
					// We shift the fragment's bbox center c=(cx,cy,cz) away from the overall model center mc,
					// so that the distance between the two will finally be scaled up by a factor of (1.0 + scale).
					pt = scale * (cx - mc);
					//Debug.Log (pt) ;
					updateAnimTransformPos (i, ref pt);
					tr.Translate (pt);
				}
			}
			//}
		}

		// FragmentList.js #765

		// Updates animation transform of a specific fragment.
		// Note: 
		//     - If scale/rotation/translation are all null, the call resets the whole transform, i.e., no anim transform is assigned anymore.
		//     - Leaving some of them null means to leave them unchanged.
		// @param { number}
		//	fragId - Fragment ID.
		// @param { Vector3 =}
		//	scale
		// @param { Quaternion=}
		//	rotationQ
		// @param { Vector3=}
		//	translation
		protected void updateAnimTransform (int fragId, ref Vector3? scale, ref Quaternion? rotationQ, ref Vector3? translation) {
			float [] ax = _animxforms;
			int off = 0;
			// Allocate animation transforms on first use.
			if ( ax == null ) {
				int count = getCount ();
				ax = _animxforms = new float [10 * count]; // 3 scale + 4 rotation + 3 translation
				for ( var i = 0 ; i < count ; i++ ) {
					// get start index of the anim transform of fragment i
					off = i * 10;
					// init as identity transform
					ax [off] = 1;        // scale.x
					ax [off + 1] = 1;    // scale.y
					ax [off + 2] = 1;    // scale.z
					ax [off + 3] = 0;    // rot.x
					ax [off + 4] = 0;    // rot.y
					ax [off + 5] = 0;    // rot.z
					ax [off + 6] = 1;    // rot.w
					ax [off + 7] = 0;    // trans.x
					ax [off + 8] = 0;    // trans.y
					ax [off + 9] = 0;    // trans.z
				}
			}
			off = fragId * 10;
			bool moved = false;
			if ( scale.HasValue ) {
				ax [off] = scale.Value.x;
				ax [off + 1] = scale.Value.y;
				ax [off + 2] = scale.Value.z;
				moved = true;
			}
			if ( rotationQ.HasValue ) {
				ax [off + 3] = rotationQ.Value.x;
				ax [off + 4] = rotationQ.Value.y;
				ax [off + 5] = rotationQ.Value.z;
				ax [off + 6] = rotationQ.Value.w;
				moved = true;
			}
			if ( translation.HasValue ) {
				ax [off + 7] = translation.Value.x;
				ax [off + 8] = translation.Value.y;
				ax [off + 9] = translation.Value.z;
				moved = true;
			}

			// Set MESH_MOVED if an animation transform has been assigned. Just if scale/rotation/translation are all null, unset the flag.
			setFlagFragment (fragId, (byte)FragmentListEnum.MESH_MOVED, moved);
			// Assume that if we are called with null everything the caller wants to reset the transform.
			if ( !moved ) {
				// Reset to identity transform
				ax [off] = 1;
				ax [off + 1] = 1;
				ax [off + 2] = 1;
				ax [off + 3] = 0;
				ax [off + 4] = 0;
				ax [off + 5] = 0;
				ax [off + 6] = 1;
				ax [off + 7] = 0;
				ax [off + 8] = 0;
				ax [off + 9] = 0;
			}
		}

		protected void updateAnimTransformFrag (int fragId) {
			float [] ax = _animxforms;
			int off = 0;
			// Allocate animation transforms on first use.
			if ( ax == null ) {
				int count = getCount ();
				ax = _animxforms = new float [10 * count]; // 3 scale + 4 rotation + 3 translation
				for ( var i = 0 ; i < count ; i++ ) {
					// get start index of the anim transform of fragment i
					off = i * 10;
					// init as identity transform
					ax [off] = 1;        // scale.x
					ax [off + 1] = 1;    // scale.y
					ax [off + 2] = 1;    // scale.z
					ax [off + 3] = 0;    // rot.x
					ax [off + 4] = 0;    // rot.y
					ax [off + 5] = 0;    // rot.z
					ax [off + 6] = 1;    // rot.w
					ax [off + 7] = 0;    // trans.x
					ax [off + 8] = 0;    // trans.y
					ax [off + 9] = 0;    // trans.z
				}
			}
			off = fragId * 10;
			bool moved = false;

			// Set MESH_MOVED if an animation transform has been assigned. Just if scale/rotation/translation are all null, unset the flag.
			setFlagFragment (fragId, (byte)FragmentListEnum.MESH_MOVED, moved);
			// Assume that if we are called with null everything the caller wants to reset the transform.
			if ( !moved ) {
				// Reset to identity transform
				ax [off] = 1;
				ax [off + 1] = 1;
				ax [off + 2] = 1;
				ax [off + 3] = 0;
				ax [off + 4] = 0;
				ax [off + 5] = 0;
				ax [off + 6] = 1;
				ax [off + 7] = 0;
				ax [off + 8] = 0;
				ax [off + 9] = 0;
			}
		}

		protected void updateAnimTransformPos (int fragId, ref Vector3 translation) {
			float [] ax = _animxforms;
			int off = 0;
			// Allocate animation transforms on first use.
			if ( ax == null ) {
				int count = getCount ();
				ax = _animxforms = new float [10 * count]; // 3 scale + 4 rotation + 3 translation
				for ( var i = 0 ; i < count ; i++ ) {
					// get start index of the anim transform of fragment i
					off = i * 10;
					// init as identity transform
					ax [off] = 1;        // scale.x
					ax [off + 1] = 1;    // scale.y
					ax [off + 2] = 1;    // scale.z
					ax [off + 3] = 0;    // rot.x
					ax [off + 4] = 0;    // rot.y
					ax [off + 5] = 0;    // rot.z
					ax [off + 6] = 1;    // rot.w
					ax [off + 7] = 0;    // trans.x
					ax [off + 8] = 0;    // trans.y
					ax [off + 9] = 0;    // trans.z
				}
			}
			off = fragId * 10;
			bool moved = false;
			//if ( translation != null ) {
			ax [off + 7] = translation.x;
			ax [off + 8] = translation.y;
			ax [off + 9] = translation.z;
			moved = true;
			//}

			// Set MESH_MOVED if an animation transform has been assigned. Just if scale/rotation/translation are all null, unset the flag.
			setFlagFragment (fragId, (byte)FragmentListEnum.MESH_MOVED, moved);
			// Assume that if we are called with null everything the caller wants to reset the transform.
			if ( !moved ) {
				// Reset to identity transform
				ax [off] = 1;
				ax [off + 1] = 1;
				ax [off + 2] = 1;
				ax [off + 3] = 0;
				ax [off + 4] = 0;
				ax [off + 5] = 0;
				ax [off + 6] = 1;
				ax [off + 7] = 0;
				ax [off + 8] = 0;
				ax [off + 9] = 0;
			}
		}

		// Applies current scale/quaternion/position to the fragment.
		//protected void updateAnimTransform () {  
		//	if ( !scale ) {
		//		scale =new Vector3 (1, 1, 1) ;
		//		quaternion =new Quaternion (0, 0, 0, 1) ;
		//		position =new Vector3 (0, 0, 0) ;
		//	}
		//	updateAnimTransform (fragId, ref scale, ref quaternion, ref position) ;
		//}

		// Returns animation transform of a specific fragment.
		// @param { number }
		//	fragId - Fragment ID.
		// @param { Vector3 =}
		//	scale - Output param.
		// @param { Quaternion =}
		//	rotationQ - Output param.
		// @param { Vector3 =}
		//	translation - Output param.
		// @returns { bool}
		//	True if an anim transform is assigned to the given fragment.
		//
		// If so, it is written to the given out params. False otherwise (outparams not changed).
		protected bool getAnimTransform (int fragId, ref Vector3? scale, ref Quaternion? rotationQ, ref Vector3? translation) {
			if ( _animxforms == null )
				return (false);
			if ( !isFlagSet (fragId, (byte)FragmentListEnum.MESH_MOVED) )
				return (false);

			var off = fragId * 10;
			var ax = _animxforms;

			if ( scale.HasValue )
				scale.Value.Set (ax [off], ax [off + 1], ax [off + 2]);
			if ( rotationQ.HasValue )
				rotationQ.Value.Set (ax [off + 3], ax [off + 4], ax [off + 5], ax [off + 6]);
			if ( translation.HasValue )
				translation.Value.Set (ax [off + 7], ax [off + 8], ax [off + 9]);

			return (true);
		}

		public int getCount () {
			return (_vizflags.Length);
		}

		public bool isFlagSet (int fragId, byte flag) {
			return ((_vizflags [fragId] & flag) != 0);
		}

		public bool setFlagFragment (int fragId, byte flag, bool value) {
			// If flag is already defined and has this value, just return false.
			byte old = _vizflags [fragId];
			bool test = !!((old & flag) != 0); // "!!" casts to boolean
			if ( test == value )
				return (false);
			// set or unset flag
			if ( value )
				_vizflags [fragId] = (byte)(old | flag);
			else
				_vizflags [fragId] = (byte)(old & ~flag);
			return (true);
		}

		public int maxDepth (GameObject node, int depth = 0) {
			depth++;
			for ( int i = 0 ; i < node.transform.childCount ; i++ ) {
				Transform tr = node.transform.GetChild (i);
				depth = Mathf.Max (depth, maxDepth (tr.gameObject, depth));
			}
			return (depth);
		}

		#endregion

	}

}
