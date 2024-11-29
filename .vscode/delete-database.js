// delete-database.js
const fs = require('fs');
const path = '../Backend/Database/skillseekdb.sqlite';

if (fs.existsSync(path)) {
    fs.unlinkSync(path);
    console.log(`Deleted ${path}`);
} else {
    console.log(`${path} does not exist.`);
}
