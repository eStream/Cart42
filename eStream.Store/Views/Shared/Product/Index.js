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

    return self;
};