Feature: MultiChannelAnalysis
	This app provides an example of how a company can optimize channel-based marketing
 
 Background:
  Given alteryx running at" http://gallery.alteryx.com/"
  And I am logged in using "deepak.manoharan@accionlabs.com" and "P@ssw0rd"

Scenario Outline: Run the multichannel analysis tool
When I run the application "<app>" with the all the Customer Segment and Product Categories and Market Area "<Market Area>"
Then I see output <result>
Examples: 
|app										  | Market Area          |                            result           |
|"Marketing - Multi-Channel Analysis Example" | "Los Angeles DMA"    |"Alteryx Report" |



