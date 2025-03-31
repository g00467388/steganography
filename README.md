# Stegano, an open source C# steganography implementation utilizing LSB encoding 
# Usage
``` sh
usage: ./stegano -m <message> -i <inputfile> -o <outputfile>
usage ./stegano -m <message> -i <inputfile> -o <outputfile>
usage: ./stegano -d <image to decrypt>
optionally use the -h flag to output an additional image displaying the modified pixels
```
# Building
## Dependencies
dotnet-sdk-9.0

dotnet-runtime-9.0
## Build
``` sh
git clone https://github.com/g00467388/steganography
cd steganography && dotnet restore && dotnet publish
```
Note This program only works on png images, other file types likely won't work
