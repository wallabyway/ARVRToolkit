#ARKit
Forge ARKit APIs


## GET health
/arkit/v1/health
This endpoint will check if the service is up and running.

**Parameters**
No parameters


**Responses**
- Response content type: application/json


|Code|Description|
|:---|:---:|
|200|OK, request successfully completed.|
|400|BAD REQUEST, The request could not be understood by the server due to malformed syntax or missing request headers. The client SHOULD NOT repeat the request without modifications. The response body may give an indication of what is wrong with the request.|
|500|INTERNAL SERVER ERROR, Internal failure while processing the request, reason depends on error.|


## GET {urn}/scenes
/arkit/v1/{urn}/scenes


## PUT {urn}/scenes/{scene_id}
/arkit/v1/{urn}/scenes/{scene_id}


## GET {urn}/scenes/{scene_id}
/arkit/v1/{urn}/scenes/{scene_id}


## DELETE {urn}/scenes/{scene_id}
/arkit/v1/{urn}/scenes/{scene_id}


## PUT {project_id}/versions/{version_id}/scenes/{scene_id}
/data/v1/projects/{project_id}/versions/{version_id}/scenes/{scene_id}


## GET {project_id}/versions/{version_id}/scenes/{scene_id}
/data/v1/projects/{project_id}/versions/{version_id}/scenes/{scene_id}


## DELETE {project_id}/versions/{version_id}/scenes/{scene_id}
/data/v1/projects/{project_id}/versions/{version_id}/scenes/{scene_id}


## POST Job
/modelderivative/v2/arkit/job


## GET {urn}/manifest
/modelderivative/v2/arkit/{urn}/manifest


## GET {urn}/scenes/{scene_id}
/modelderivative/v2/arkit/{urn}/scenes/{scene_id}

## GET {urn}/mesh/{dbId}/{fragId}
/modelderivative/v2/arkit/{urn}/mesh/{dbId}/{fragId}

## GET {urn}/scenes/{scene_id}/mesh/{dbId}/{fragId}
/modelderivative/v2/arkit/{urn}/scenes/{scene_id}/mesh/{dbId}/{fragId}

## GET {urn}/material/{matId}/{mat}
/modelderivative/v2/arkit/{urn}/material/{matId}/{mat}

## GET {urn}/texture/{tex}
/modelderivative/v2/arkit/{urn}/texture/{tex}

## GET {urn}/properties/{dbIds}
/modelderivative/v2/arkit/{urn}/properties/{dbIds}

## GET {urn}/bubble
/modelderivative/v2/arkit/{urn}/bubble

## GET {urn}/unity
/modelderivative/v2/arkit/{urn}/unity
