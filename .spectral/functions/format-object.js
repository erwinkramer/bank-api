import { createRulesetFunction } from "@stoplight/spectral-core";

export default createRulesetFunction(
    {
        input: null,
        options: {
            type: "object",
            additionalProperties: false,
            properties: {
                elementName: true,
                requiredElements: {
                    type: "array",
                    items: { type: "string" },
                    default: []
                },
                optionalElements: {
                    type: "array",
                    items: { type: "string" },
                    default: []
                },
            },
            required: ["elementName", "requiredElements"],
        },
    },
    (targetVal, options) => {
        const { elementName, requiredElements = [], optionalElements = [] } = options;

        if (typeof targetVal === "object" && targetVal[elementName]) {
            const allAllowedElements = [...requiredElements, ...optionalElements];
            const results = [];

            // Check for missing required elements
            const missingElements = requiredElements.filter(
                (element) => !targetVal[elementName][element]
            );
            if (missingElements.length > 0) {
                missingElements.forEach((element) => {
                    results.push({
                        message: `${elementName} must contain an element named "${element}".`,
                    });
                });
            }

            // Check for unexpected elements
            const unexpectedElements = Object.keys(targetVal[elementName]).filter(
                (element) => !allAllowedElements.includes(element)
            );
            if (unexpectedElements.length > 0) {
                unexpectedElements.forEach((element) => {
                    results.push({
                        message: `${elementName} contains an unexpected element named "${element}".`,
                    });
                });
            }

            return results;
        }

        // Return an empty array if no issues are found
        return [];
    }
);