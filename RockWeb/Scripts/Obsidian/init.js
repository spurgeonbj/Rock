(function () {
    window.Obsidian = window.Obsidian || {};

    Obsidian.Util = {
        getGuid: () => 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : r & 0x3 | 0x8;
            return v.toString(16);
        }),
        getBlockActionFunction: ({ blockId, pageId }) => {
            return async (actionName, data) => {
                try {
                    return await axios.post(`/api/blocks/action/${pageId}/${blockId}/${actionName}`, data);
                }
                catch (e) {
                    if (e.response && e.response.data && e.response.data.Message) {
                        throw e.response.data.Message;
                    }

                    throw e;
                }
            };
        },
        isSuccessStatusCode: (statusCode) => statusCode && statusCode / 100 === 2
    };

    Obsidian.Blocks = {};

    Vue.prototype.$http = axios;
})();
