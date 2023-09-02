var currentFolder = __dirname;
var argumentFolderName = process.argv[2];
var md = require('markdown-it')({
    html: true,
    linkify: true,
    typographer: true
});
const fs = require('fs');
if (!fs.existsSync(argumentFolderName)) {
    fs.mkdirSync(argumentFolderName);
}
var allFilesInCurrentFolder = fs.readdirSync(currentFolder).filter(file => file.endsWith('.md'));
allFilesInCurrentFolder.forEach(function (file) {
    var data = fs.readFileSync(file, 'utf8');
    var result = md.render(data);
    var filePath= currentFolder + "\\" + argumentFolderName + "\\" + file;
    fs.writeFile(filePath, result, function (err) {
        if(err) {
            return console.log(err);
        }
        console.log(`Parsed ${filePath}`);
    });
});

