### What is Xiletrade.Json?
Xiletrade.Json is a tiny console app used to generate json files consumed by [Xiletrade price checker](https://github.com/maxensas/xiletrade)  
Developed under .NET8 C# and made available to all tool developers as an example of using [libGGPK3](https://github.com/aianlinb/LibGGPK3) by [aianlinb](https://github.com/aianlinb)

### Prerequisites for use :
Like all other PoE tools, DAT Schemas located in 'DatDefinitions.json' need to be updated at each PoE release that adds breaking changes.

### Main feature :
This app provide a CSV to JSON parser in order to get the smallest json files that Xiletrade need for a smooth running in all languages.
It is also possible to extract DAT64 and CSV relative files. 

### Future possibilities :
* Update to a CLI tool.
* Ability to merge data from other DAT files using table references. 

## For further informations :
This program is free, open-source, licensed under MIT License.  

## Special thanks
Benevolent developers who participated in various fan projects since the first PoE release and the authors behind libraries used in this example :
* [libGGPK3](https://github.com/aianlinb/LibGGPK3) by [aianlinb](https://github.com/aianlinb)
* [libDat2](https://github.com/aianlinb/LibGGPK2/tree/master/LibDat2) by [aianlinb](https://github.com/aianlinb)
* [CsvHelper](https://github.com/JoshClose/CsvHelper) by [Josh Close](https://github.com/JoshClose)

## License for generated files
Every output generated come from GGG (game publisher) archive files. Xiletrade have no license on these.

## Screenshots
* Console :
<img src="https://user-images.githubusercontent.com/62154281/219967076-3b28dffc-94aa-4ba1-b771-832e3853e997.png" width="60%" height="60%"> 
