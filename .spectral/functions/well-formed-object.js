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
            required: ["elementName"],
        },
    },
    (targetVal, options) => {
        const { elementName, requiredElements = [], optionalElements = [] } = options;

        if (typeof targetVal === "object" && targetVal[elementName]) {
            const allAllowedElements = [...requiredElements, ...optionalElements];
            const results = [];

            // Check if 'properties' exists inside the element
            if (!targetVal[elementName].properties) {
                results.push({
                    message: `${elementName} must contain a 'properties' element.`,
                });
                return results;
            }

            const properties = targetVal[elementName].properties;

            // Check for missing required elements inside 'properties'
            const missingElements = requiredElements.filter(
                (element) => !properties[element]
            );
            if (missingElements.length > 0) {
                missingElements.forEach((element) => {
                    results.push({
                        message: `${elementName}.properties must contain an element named "${element}".`,
                    });
                });
            }

            // Check for unexpected elements inside 'properties'
            const unexpectedElements = Object.keys(properties).filter(
                (element) => !allAllowedElements.includes(element)
            );
            if (unexpectedElements.length > 0) {
                unexpectedElements.forEach((element) => {
                    results.push({
                        message: `${elementName}.properties contains an unexpected element named "${element}".`,
                    });
                });
            }

            return results;
        }

        // Return an empty array if no issues are found
        return [];
    }
);