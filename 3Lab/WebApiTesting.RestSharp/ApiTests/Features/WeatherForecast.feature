Feature: Weather forecast
	Category Weather of the public API list: service Open-Meteo.
	Laboratory work 3, task 2.

Background:
	Given the weather service is used

@weather @smoke
Scenario: Current weather is returned for the given coordinates
	When I ask for the current weather in Lviv
	Then the response status is 200
	And the answer contains the current temperature
	And the returned coordinates are close to the requested ones

# Кількість днів змінюється, точка стала. Значення 16 це верхня межа діапазону.
@weather
Scenario Outline: Daily forecast returns as many days as requested
	When I ask for a daily forecast for <days> days
	Then the response status is 200
	And the forecast contains exactly <days> dates

Examples:
	| days |
	| 1    |
	| 3    |
	| 16   |

@weather
Scenario: Forecast with the automatic time zone returns the local zone
	When I ask for a daily forecast for 3 days
	Then the time zone of the answer is "Europe/Kyiv"

# Одиниця змінюється, решта запиту стала.
@weather
Scenario Outline: Temperature is returned in the requested unit
	When I ask for the current weather in the unit "<unit>"
	Then the unit of the temperature is "<symbol>"

Examples:
	| unit       | symbol |
	| celsius    | °C     |
	| fahrenheit | °F     |

# Аналіз граничних значень: 90 і 180 приймаються, крок за межу відхиляється.
@weather
Scenario Outline: Coordinates on the edge of the allowed range are accepted
	When I ask for the current weather at latitude <latitude> and longitude <longitude>
	Then the response status is 200

Examples:
	| latitude | longitude |
	| 90       | 24.03     |
	| -90      | 24.03     |
	| 49.84    | 180       |

@weather @negative
Scenario Outline: Coordinates outside the allowed range are rejected
	When I ask for the current weather at latitude <latitude> and longitude <longitude>
	Then the response status is 400
	And the refusal reason mentions "<field>"

Examples:
	| latitude | longitude | field     |
	| 91       | 24.03     | Latitude  |
	| -91      | 24.03     | Latitude  |
	| 49.84    | 181       | Longitude |

@weather @negative
Scenario: Request without the longitude is rejected
	When I ask for the current weather with the latitude only
	Then the response status is 400
	And the refusal reason mentions "longitude"

@weather @negative
Scenario: Forecast longer than the allowed range is rejected
	When I ask for a daily forecast for 17 days
	Then the response status is 400
	And the refusal reason mentions "Forecast days"
