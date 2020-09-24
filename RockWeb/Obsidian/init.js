(function () {
    window.Obsidian = window.Obsidian || {};

    Obsidian.Util = {
        loadVueFile: loadVueComponent,
        getGuid: () => 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : r & 0x3 | 0x8;
            return v.toString(16);
        }),
        getBlockActionFunction: ({ blockGuid, pageGuid }) => {
            return async (actionName, data) => {
                try {
                    return await Obsidian.Http.post(`/api/blocks/action/${pageGuid}/${blockGuid}/${actionName}`, undefined, data);
                }
                catch (e) {
                    if (e.response && e.response.data && e.response.data.Message) {
                        throw e.response.data.Message;
                    }

                    throw e;
                }
            };
        },
        getBlockHttp: ({ blockGuid }) => {
            const call = (method, url, params, data) => {
                Obsidian.BlockLog[blockGuid] = Obsidian.BlockLog[blockGuid] || [];
                const log = Obsidian.BlockLog[blockGuid];
                log.push(method);

                return axios({
                    method,
                    url,
                    data,
                    params
                });
            };

            return {
                get: (url, params) => {
                    return call('GET', url, params);
                },
                post: (url, params, data) => {
                    return call('POST', url, params, data);
                }
            };
        },
        isSuccessStatusCode: (statusCode) => statusCode && statusCode / 100 === 2
    };

    Obsidian.Elements = {};
    Obsidian.Controls = {};
    Obsidian.Blocks = {};
    Obsidian.BlockLog = {};
})();
