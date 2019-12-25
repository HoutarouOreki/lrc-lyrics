const searchTextBox = document.getElementById('search-textbox');
const searchResults = document.getElementById('search-results')

searchTextBox.addEventListener('keydown', function (e) {
    if (e.keyCode == 13) {
        searchRequested(e);
    }
});

function searchRequested(e) {
    // while (searchResults.firstChild) {
    //     searchResults.removeChild(searchResults.firstChild);
    // }
    var searchEntryContainer = document.createElement('div');
    searchEntryContainer.className = 'search-entry-container';
    
    var searchEntryContent = document.createElement('div');
    searchEntryContent.className = 'search-entry';
    searchEntryContent.textContent = searchTextBox.value;
    console.log(searchTextBox.value);
    searchEntryContainer.appendChild(searchEntryContent);

    searchResults.appendChild(searchEntryContainer);
}