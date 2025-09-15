document.getElementById('searchButton').addEventListener('click', function() {
    const userId = document.getElementById('userId').value;
    const searchType = document.getElementById('searchType').value;
    const searchQuery = document.getElementById('searchQuery').value;

    if (!userId) {
        alert('User ID is required.');
        return;
    }

    fetch(`/api/book/searchby?userId=${userId}&searchType=${searchType}&searchQuery=${searchQuery}`)
        .then(response => response.json())
        .then(data => {
            const tbody = document.querySelector('#bookTable tbody');
            const noResultsMessage = document.getElementById('noResultsMessage');
            tbody.innerHTML = ''; // Очистить таблицу перед добавлением новых данных

            if (data.length === 0) {
                noResultsMessage.style.display = 'block';
            } else {
                noResultsMessage.style.display = 'none';
                data.forEach(book => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                        <td>${book.id}</td>
                        <td>${book.title}</td>
                        <td>${book.author}</td>
                        <td>${book.genres.join(', ')}</td>
                        <td>${book.publicationYear}</td>
                        <td>${book.annotation}</td>
                        <td>${book.isbn}</td>
                        <td>${book.userId}</td>
                    `;
                    tbody.appendChild(row);
                });
            }
        })
        .catch(error => console.error('Error searching books:', error));
});