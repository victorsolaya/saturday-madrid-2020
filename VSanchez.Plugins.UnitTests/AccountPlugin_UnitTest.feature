Feature: AccountPlugin_UnitTest
	In order to check the update of the city and country
	As an user
	I want to be shown the result as the account

Background:
	Given the entity "account" known as Axazure exists with the fields
		| Field            | Value   |
		| name             | Axazure |
		| address1_city    | Madrid  |
		| address1_country | Spain   |
	And the entity "contact" known as VictorSanchez exists with the fields
		| Field            | Value          |
		| parentcustomerid | Axazure        |
		| firstname        | Victor         |
		| lastname         | Sanchez        |
		| fullname         | Victor Sanchez |
		| vso_dni          | 1235561G       |
		| address1_city    | Glasgow        |
		| address1_country | Scotland       |
	And the entity "contact" known as JuanBarbanegra exists with the fields
		| Field            | Value           |
		| parentcustomerid | Axazure         |
		| firstname        | Juan            |
		| lastname         | Barbanegra      |
		| fullname         | Juan Barbanegra |
		| vso_dni          | 1425561G        |
		| address1_city    | Paris           |
		| address1_country | Francia         |
	And the entity name contains primary attribute name
		| Field   | Value    |
		| account | name     |
		| contact | fullname |

@AccountUpdate
Scenario: Change the city for an account
	When I call the plugin AccountPlugin with record "Axazure"
	And all the entities are refreshed
	Then the service should have 2 records of the entity contact
	And the entity VictorSanchez should have fields
		| Field            | Value    |
		| parentcustomerid | Axazure  |
		| firstname        | Victor   |
		| lastname         | Sanchez  |
		| vso_dni          | 1235561G |
		| address1_city    | Madrid   |
		| address1_country | Spain    |
	And the entity JuanBarbanegra should have fields
		| Field            | Value      |
		| parentcustomerid | Axazure    |
		| firstname        | Juan       |
		| lastname         | Barbanegra |
		| vso_dni          | 1425561G   |
		| address1_city    | Madrid     |
		| address1_country | Spain      |