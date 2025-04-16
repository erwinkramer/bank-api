export default input => {
    if (typeof input === 'object' && input.bankTier) {
        const requiredElements = ['enum', 'description'];
        const optionalElements = ['default'];
        const allAllowedElements = [...requiredElements, ...optionalElements];
        const results = [];

        // Check for missing required elements
        const missingElements = requiredElements.filter(element => !input.bankTier[element]);
        if (missingElements.length > 0) {
            missingElements.forEach(element => {
                results.push({
                    message: `bankTier must contain an element named "${element}".`,
                });
            });
        }

        // Check for unexpected elements
        const unexpectedElements = Object.keys(input.bankTier).filter(
            element => !allAllowedElements.includes(element)
        );
        if (unexpectedElements.length > 0) {
            unexpectedElements.forEach(element => {
                results.push({
                    message: `bankTier contains an unexpected element named "${element}".`,
                });
            });
        }

        return results;
    }
};