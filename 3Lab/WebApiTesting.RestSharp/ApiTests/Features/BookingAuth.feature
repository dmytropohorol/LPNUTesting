Feature: Booking authorisation
	Access control over the write operations of restful-booker.
	Laboratory work 3, task 1.

Background:
	Given the booking service is available

@auth
Scenario: Correct credentials return an access token
	When I request a token with the correct credentials
	Then an access token of 15 characters is returned

@auth @negative
Scenario: Wrong password does not return a token
	When I request a token with the password "wrong-password"
	Then no token is returned
	And the refusal reason is "Bad credentials"

@auth @negative
Scenario: Unknown user does not return a token
	When I request a token with the user name "nobody"
	Then no token is returned
	And the refusal reason is "Bad credentials"

# Запис без маркера доступу не повинен проходити. Змінюється лише операція,
# бронювання та решта умов однакові.
@auth @negative
Scenario Outline: Write operation without a token is forbidden
	Given a created booking
	When I send a <operation> request without a token
	Then the response status is 403
	And the booking is unchanged

Examples:
	| operation |
	| replace   |
	| change    |
	| delete    |
