# SolarView
An Azure SaaS Product that collects solar data from a SolarEdge account for future analysis.

# What is SolarView?
[SolarEdge](https://www.solaredge.com/) provide a free mobile application called [mySolarEdge](https://www.solaredge.com/mysolaredge) that provides real-time tracking of household energy use and production.

[mySolarEdge](https://www.solaredge.com/mysolaredge) includes a number of graphs that provide insight into historical weekly, monthly, and yearly data. There's one view, however, that it doesn't provide; an average 24-hour view across all data.

**SolarView** was primarily implemented as an exercise to become familar with Azure Functions and their interaction with CosmosDB, Azure Table, ServiceBus, and SendGrid. The end result is a product that provides the following functionality:

* Upon initial setup, collects all available data since the installation date
* Continues to collect data on the hour, every hour
* Daily data is stored in a CosmosDB collection
* A CosmosDB trigger detects new documents and processes them to decompose the data down to individual data points (per meter type, per 15 minutes)
* Maintains a history of data refreshes in an Azure Table
* Tracks the last refresh time for each configured site (timezone specific)
* Sends a daily email to each site owner that provides a summary for the past 24 hours
* Regularly calculates data for an average 24 hour window based on weekly, monthly, and yearly data

# Architecture
*TODO: Add diagram(s) and explanation*

# UI
*TODO: Consider UI options / features for retrieving and displaying the data*