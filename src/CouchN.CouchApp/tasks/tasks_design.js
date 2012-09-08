var couchapp = require('couchapp')
 , path = require('path')
;

ddoc =
    {
        rewrites: [],
        views: {},
        lists: {},
        shows: {},
        updates: {}
    };

ddoc.views.by_id =
    {
        map: function (doc) {
            if (doc.type == 'Task')
                emit(doc._id);
        }
    };


ddoc.validate_doc_update = function (newDoc, oldDoc, userCtx) {
    if (newDoc._deleted === true && userCtx.roles.indexOf('_admin') === -1) {
        throw "Only admin can delete documents on this database.";
    }
};

module.exports = ddoc;