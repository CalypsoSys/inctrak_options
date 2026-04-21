(function (window) {
    function defaultApiBaseUrl() {
        var hostname = window.location.hostname;
        if (hostname === 'localhost' || hostname === '127.0.0.1') {
            return 'https://localhost:5001';
        }

        return 'https://shared.inctrak.com';
    }

    function trimSlash(value, trimStart) {
        return trimStart ? value.replace(/^\/+/, '') : value.replace(/\/+$/, '');
    }

    function buildApiUrl(path) {
        if (!path || path.indexOf('/api/') !== 0) {
            return path;
        }

        var config = window.IncTrakSiteConfig || {};
        var apiBaseUrl = config.apiBaseUrl || defaultApiBaseUrl();
        return trimSlash(apiBaseUrl, false) + '/' + trimSlash(path, true);
    }

    window.IncTrakSiteConfig = window.IncTrakSiteConfig || {
        apiBaseUrl: defaultApiBaseUrl()
    };

    window.IncTrakSite = {
        apiUrl: buildApiUrl
    };
})(window);
