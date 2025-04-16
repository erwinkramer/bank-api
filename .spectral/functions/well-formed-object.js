import { createRulesetFunction } from "@stoplight/spectral-core";

export default createRulesetFunction(
    {
        input: null,
        options: {
            type: "object",
            additionalProperties: false,
            properties: {
                elementName: true,
                requiredProperties: {
                    type: "array",
                    items: { type: "string" },
                    default: []
                },
                optionalProperties: {
                    type: "array",
                    items: { type: "string" },
                    default: []
                },
            },
            required: ["elementName"],
        },
    },
    (targetVal, options) => {
        const { elementName, requiredProperties = [], optionalProperties = [] } = options;

        if (typeof targetVal === "object" && targetVal[elementName]) {
            const allAllowedProperties = [...requiredProperties, ...optionalProperties];
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
            const missingProperties = requiredProperties.filter(
                (element) => !properties[element]
            );
            if (missingProperties.length > 0) {
                missingProperties.forEach((element) => {
                    results.push({
                        message: `${elementName}.properties must contain an element named "${element}".`,
                    });
                });
            }

            // Check for unexpected elements inside 'properties'
            const unexpectedProperties = Object.keys(properties).filter(
                (element) => !allAllowedProperties.includes(element)
            );
            if (unexpectedProperties.length > 0) {
                unexpectedProperties.forEach((element) => {
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