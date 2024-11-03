# CSS-Benchmark
 A CS# Benchmark Plugin

# Installation
 Simply drag and drop the addons folder into your server game directory

# Configuration
 Set the LogInterval in ms

# Usage
 Use `css_startbenchmark` to start logging data

 Use `css_stopbenchmark` to stop logging data

 The memory and player count data will be stored in /game/csgo/benchmark/benchmark-{currentTime}.json
 and the frametime data will be handled by VProf in the console log 

# Graphing
 To graph this data, I use https://jsontochart.com/
 
 Encompass the entire contents of the benchmark data in [ ] to comply by json formatting
 
 Set X axis to Index, then add the rest as value fields
