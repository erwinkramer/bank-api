import { createRulesetFunction } from '@stoplight/spectral-core';

/**
 * @author Roberto Polli <https://github.com/ioggstream>
 */
const getAllOperations = function* (paths) {
    if (typeof paths !== "object") {
        return;
    }
    const validMethods = [
        "get",
        "head",
        "post",
        "put",
        "patch",
        "delete",
        "options",
        "trace",
    ];
    const operation = { path: "", operation: "" };
    for (const idx of Object.keys(paths)) {
        const path = paths[idx];
        if (typeof path === "object") {
            operation.path = idx;
            for (const httpMethod of Object.keys(path)) {
                typeof path[httpMethod] === "object" &&
                    validMethods.includes(httpMethod) &&
                    ((operation.operation = httpMethod), yield operation);
            }
        }
    }
};
var differentSecuritySchemes = createRulesetFunction({
    input: null,
    options: {
        type: "object",
        additionalProperties: false,
        properties: {
            adminUrl: {
                type: "string",
            },
        },
        required: [],
    },
}, function differentSecuritySchemes(input, options) {
    const errorList = [];
    const nonAdminSecurityHashes = [];
    const adminSecurityEntries = [];
    const { adminUrl = "/admin" } = options;
    const { paths } = input;
    for (const { path, operation: httpMethod } of getAllOperations(paths)) {
        let { security: operationSecurity } = paths[path][httpMethod];
        // No security so skip this and leave it for other rules which check for security.
        if (operationSecurity === undefined) {
            continue;
        }
        if (Array.isArray(operationSecurity)) {
            for (const [idx, securityEntry] of operationSecurity.entries()) {
                if (typeof securityEntry !== "object") {
                    continue;
                }
                // This creates a string for easier comparison to see if its used elsewhere, but
                // this might not be tough enough for some of the weirder edge cases.
                const securityHash = JSON.stringify(securityEntry);
                if (path.includes(adminUrl)) {
                    adminSecurityEntries.push({
                        uri: path,
                        hash: securityHash,
                        path: ["paths", path, httpMethod, "security", idx],
                    });
                }
                else {
                    nonAdminSecurityHashes.push(securityHash);
                }
            }
        }
    }
    // For every admin security entry, check if it is used in a non-admin path.
    for (const { uri, hash, path } of adminSecurityEntries) {
        if (nonAdminSecurityHashes.includes(hash)) {
            errorList.push({
                message: `Admin endpoint ${uri} has the same security requirement as a non-admin endpoint.`,
                path,
            });
        }
    }
    return errorList;
});

export { differentSecuritySchemes as default };
