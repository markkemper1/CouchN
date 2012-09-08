var couchapp = require('couchapp')
 , path = require('path')
 , watch = require('watch');

var dir = __dirname;

console.log(dir);

watch.walk(dir, { ignoreDotFiles: true },
    function (err, files) {
        for (i in files)
        {
            var fullFilename = i;
            var filename = path.basename(i);

            if (filename.indexOf("_design.js") < 0) continue;

            if (path.extname(fullFilename) != ".js") continue;
            
            var docId = "_design/" + filename.replace("_design.js", "");

            var requireNameDir = path.relative(dir, path.dirname(fullFilename));
            var requireName = path.join(requireNameDir, path.basename(fullFilename));
            requireName = './' + requireName;

            var doc = require(requireName);
            doc._id = docId;
            
            doc.exportFilename = path.join(requireNameDir, path.basename(filename, ".js") + ".json");
            
            console.log('Design Doc: ' + doc._id + ", found here: " + fullFilename);
            couchapp.createApp(doc, '', function (app) { app.push(); } );
        }
    }
);
