Feature: Booking CRUD
	Operations of type CRUD over the /booking resource of restful-booker.
	Laboratory work 3, task 1.

Background:
	Given the booking service is available
	And I have an access token

@smoke @crud
Scenario: Booking passes the full life cycle
	When I create a booking
	Then the booking is created and has a number
	When I read the created booking
	Then the booking matches the data I sent
	When I replace the booking with other data
	Then the booking matches the replacement data
	When I delete the booking
	Then the booking is gone

@crud
Scenario: Created booking appears in the list of all bookings
	When I create a booking
	And I read the list of booking numbers
	Then the list contains the created booking number

@crud
Scenario: Created booking is found by the customer name
	When I create a booking
	And I search bookings by the customer name
	Then the list contains the created booking number

@crud
Scenario: Partial update changes only the given field
	When I create a booking
	And I change only the total price to 555
	Then the total price equals 555
	And the other fields keep the values I sent

@crud @negative
Scenario: Reading a booking that does not exist returns Not Found
	When I read a booking that does not exist
	Then the response status is 404

@crud @negative
Scenario: Creating a booking without the required fields is rejected
	When I create a booking with the first name only
	Then the request is rejected

# Запис у неіснуючий ресурс. Змінюється лише операція.
@crud @negative
Scenario Outline: Writing to a booking that does not exist is rejected
	When I send a <operation> request to a booking that does not exist
	Then the request is rejected

Examples:
	| operation |
	| replace   |
	| delete    |
