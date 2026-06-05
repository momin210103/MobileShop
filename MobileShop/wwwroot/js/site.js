// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    // Search Auto-suggestions
    var searchTimeout;
    $('#searchInput').on('input', function () {
        clearTimeout(searchTimeout);
        var term = $(this).val();

        if (term.length < 2) {
            $('#searchSuggestions').hide();
            return;
        }

        searchTimeout = setTimeout(function () {
            $.get('/Products/Search', { term: term }, function (data) {
                if (data.length > 0) {
                    var html = '';
                    data.forEach(function (item) {
                        html += `<a href="/Products/Details/${item.id}" class="list-group-item list-group-item-action d-flex align-items-center">
                            <img src="${item.image || 'https://via.placeholder.com/40x40?text=No+Image'}" class="rounded me-3" style="width: 40px; height: 40px; object-fit: cover;" />
                            <div>
                                <h6 class="mb-0">${item.name}</h6>
                                <small class="text-muted">${item.brand} - RS ${item.price.toLocaleString()}</small>
                            </div>
                        </a>`;
                    });
                    $('#suggestionsList').html(html);
                    $('#searchSuggestions').show();
                } else {
                    $('#searchSuggestions').hide();
                }
            });
        }, 300);
    });

    // Hide suggestions when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#searchInput').length) {
            $('#searchSuggestions').hide();
        }
    });

});