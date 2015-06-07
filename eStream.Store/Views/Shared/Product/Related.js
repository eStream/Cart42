function ProductIndexViewModel(data) {
    var self = this;
    ko.mapping.fromJS(data, {}, self);
    self.goToPage = function (pageNum) {
        if (pageNum < 1 || pageNum > self.totalPages()) return;
        $.post("/Product/List", {
            categoryId: self.categoryId(),
            keywords: self.keywords(),
            featured: self.featured(),
            page: pageNum
        },
            function (json) {
                ko.mapping.fromJS(json, {}, self);
            });
    };

    self.productRows = function (productsPerRow) {
        var products = self.products();
        var result = [];
        var row = [];
        for (var i = 0; i < products.length; i++) {
            row.push(products[i]);
            if ((i + 1) % productsPerRow == 0 || i == products.length - 1) {
                result.push(row);
                row = [];
            }
        }
        return result;
    }

    return self;
};