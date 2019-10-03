#ESPNBot
##Jeff Jewett

###What is ESPNBot?
ESPNBot is a espn.com fantasy football automation tool. It utilizes Selenium to navigate espn.com to perform roster actions.
Its current directive is to scan for players that are out, whether by injury or bye-week, then substitute the highest projected
player of the same position from the bench.

If you want to modify its management algorithm, modify RosterManagement.cs, specifically ManageTeam(int currWeek).