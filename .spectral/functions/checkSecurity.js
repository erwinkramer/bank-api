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
var checkSecurity = createRulesetFunction({
    input: null,
    options: {
        type: "object",
        additionalProperties: false,
        properties: {
            schemesPath: {
                type: "array",
            },
            nullable: true,
            methods: {
                type: "array",
            },
        },
        required: [],
    },
}, function checkSecurity(input, options) {
    const errorList = [];
    const { schemesPath: s, nullable, methods } = options;
    const { paths, security } = input;
    for (const { path, operation: httpMethod } of getAllOperations(paths)) {
        // Skip methods not configured in `methods`.
        if (methods && Array.isArray(methods) && !methods.includes(httpMethod)) {
            continue;
        }
        let { security: operationSecurity } = paths[path][httpMethod];
        let securityRef = [path, httpMethod];
        if (operationSecurity === undefined) {
            operationSecurity = security;
            securityRef = ["$.security"];
        }
        if (!operationSecurity || operationSecurity.length === 0) {
            errorList.push({
                message: `Operation has undefined security scheme in ${securityRef}.`,
                path: ["paths", path, httpMethod, "security", s],
            });
        }
        if (Array.isArray(operationSecurity)) {
            for (const [idx, securityEntry] of operationSecurity.entries()) {
                if (typeof securityEntry !== "object") {
                    continue;
                }
                const securitySchemeIds = Object.keys(securityEntry);
                securitySchemeIds.length === 0 &&
                    nullable === false &&
                    errorList.push({
                        message: `Operation referencing empty security scheme in ${securityRef}.`,
                        path: ["paths", path, httpMethod, "security", idx],
                    });
            }
        }
    }
    return errorList;
});

export { checkSecurity as default };
