# Task No. 7 - Document any recommendations on improving the structure or maintainability of the code

## To complete this assessment, the following considerations were assumed:

- Not modifying the data structure of the pre-existing solution. In a production environment, we should not do more than the approved and assigned task; however, since I do not know the evaluation criteria, I decided to implement the new functionalities while trying to follow good practices in object-oriented programming and SOLID principles. The new functionalities must coexist with the existing ones, regardless of whether they are implemented differently.
- For Task No. 4 (The delete endpoint should make the next member on the account the primary member): Since there is no criterion to determine who the next member should be, I assumed the next member to be any other member of the account.

## Considerations of the pre-existing solution

- We should consider assigning a single responsibility to the LocationsController. This controller should be exclusively responsible for handling API requests, not for including the implementation of these calls.
- It would be advisable to remove the LocationDto object from this controller. Folders could be created to store model objects and DTO objects related to Location.
- Avoid using a generic LocationDto object. Create one per type of operation. Example: LocationCreateDto, LocationUpdateDto, and LocationReadDto. This ensures that the API exposes only a specific portion of the object in each context as appropriate, and not the entire entity.
- Include exception handling for the LocationController.
- Standardize API response codes according to the result of the method execution, depending on whether the call was successful or not. Some codes are still missing.
- Although the current solution is functional, a service layer could be created for Location. This layer would be responsible for receiving requests from the Location controller and applying business logic.
- It may also be worth considering creating a repository to handle basic data access operations for the Location entity—CRUD operations and others. This repository would be injected into the service. The structure would be Controller → Service → Repository.
- Standardize the URLs for the endpoints in LocationsController to comply with RESTful standards.
- Create XML comments to describe each endpoint or method in the solution. This improves understanding of the method for other team members and also helps expose endpoint functionalities in a standardized way using API documentation tools such as Swagger.
- Serilog is continuously running on sensitive events, but it may be useful to add specific events defined by the team that are important to monitor.
- When deleting a Location, validation should be added to determine what to do with accounts and members referencing that location to avoid leaving orphaned records. Responsibility could be delegated to cascading deletes in the database, removing an element and its hierarchy, but this could also be dangerous. An application-level validation could be implemented to verify that no associated records exist before deleting the location.
- Validations could be included in Create and Update operations to avoid duplicate or incomplete data.

## Recommendations for the Current Solution (Assessment completed)

- For Task No. 4 (The delete endpoint should make the next member on the account the primary member): It should have been validated that the next member should not be cancelled (Cancelled = true).
- Transform the Locations controller so that it makes calls to a service layer. This service layer will manage a Location repository with data access methods for the Location entity. The structure would be: LocationsController → LocationService → LocationRepository.
- Create the DTOs for Location.
- Create and include custom exceptions in the solution, such as the existing ones: LastAccountMemberException and PrimaryMemberException.
- Dependency validations with other records should be included when deleting Accounts and Members to ensure no orphaned records remain. This is not implemented because it was not a specific task of the assessment, but it should be implemented.
- Create abstractions for the models; we should depend on abstractions and not concrete classes to respect SOLID principles.
- Consider improving interface segregation by dividing service and repository layers into multiple services and repositories, each implementing a single functionality. This way, clients are not exposed to complex interfaces with functionality they do not need.

## Recommendations for the solution in general:

- Install Swagger to improve API documentation and provide a standardized visual interface to consume the different endpoints. This includes defining examples.
- Create indexes in the database tables following the recommendations of Task No. 6.
- Implement unit tests using XUnit.