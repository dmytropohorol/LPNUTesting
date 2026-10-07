Feature: Open Account
	Bank manager of XYZ Bank opens a new account for an existing customer.
	Laboratory work 2, variant 4: Bank Manager Login, operation Open Account.

Background:
	Given the XYZ Bank application is open
	And I am logged in as Bank Manager

@smoke @variant4
Scenario: Account is opened for an existing customer
	When I open the Open Account tab
	And I select customer "Harry Potter"
	And I select currency "Dollar"
	And I process the account opening
	Then the account creation message is shown
	And the created account number is not empty

# Клієнт сталий, міняється тільки валюта. Dollar уже перевірено вище.
@regression @variant4
Scenario Outline: Account is opened in any supported currency
	When I open the Open Account tab
	And I select customer "Harry Potter"
	And I select currency "<currency>"
	And I process the account opening
	Then the account creation message is shown

Examples:
	| currency |
	| Pound    |
	| Rupee    |

# Валюта стала, міняється тільки клієнт. Harry Potter уже перевірений вище.
@regression @variant4
Scenario Outline: Account is opened for any bank customer
	When I open the Open Account tab
	And I select customer "<customer>"
	And I select currency "Dollar"
	And I process the account opening
	Then the account creation message is shown

Examples:
	| customer           |
	| Hermoine Granger   |
	| Ron Weasly         |
	| Albus Dumbledore   |
	| Neville Longbottom |

@negative @variant4
Scenario: Account is not opened when customer is not selected
	When I open the Open Account tab
	And I select currency "Dollar"
	And I process the account opening
	Then no account creation message is shown
	And I stay on the Open Account page

@negative @variant4
Scenario: Account is not opened when currency is not selected
	When I open the Open Account tab
	And I select customer "Ron Weasly"
	And I process the account opening
	Then no account creation message is shown
	And I stay on the Open Account page

@ui @variant4
Scenario: Customer list contains all bank customers
	When I open the Open Account tab
	Then the customer list contains:
		| customer           |
		| Hermoine Granger   |
		| Harry Potter       |
		| Ron Weasly         |
		| Albus Dumbledore   |
		| Neville Longbottom |
