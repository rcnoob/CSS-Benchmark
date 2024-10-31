# CSS-Benchmark
 A CS# Benchmark Plugin

# Installation
 Simply drag and drop the plugin into your plugins folder for CS# (v284+)

# Usage
 After installing, restart your server to freshly initialize the benchmark.
 
 Allow the server to run
 
 Once finished, disable the plugin by either deleting it or creating a "disabled" folder, then drag into there
 
 Your benchmark data will be stored in /game/csgo/benchmark/benchmark-{currentTime}.json

# Graphing
 To graph this data, I use https://jsontochart.com/
 
 Encompass the entire contents of the benchmark data in [ ] to comply by json formatting
 
 Set X axis to Index, then add the value fields of AvgPlayerCount, AvgFrameTimeTicks, and FrameTimeTicksPeak
