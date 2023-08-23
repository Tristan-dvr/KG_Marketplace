var argumentFileName = process.argv[2];
var argumentNewFileName = process.argv[3];
var md = require('markdown-it')({
    html: true,
    linkify: true,
    typographer: true
});
const fs = require('fs');
var data = fs.readFileSync(argumentFileName+".md", 'utf8');
var result = md.render(data);
fs.writeFile(argumentNewFileName+".md", result, function(err) {
    if(err) {
        return console.log(err);
    }
    console.log("The file was saved!");
});
