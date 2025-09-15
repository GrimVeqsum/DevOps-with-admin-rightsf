document.getElementById('fetchBooksButton').addEventListener('click', function() {
    fetch('/api/book/getbooks')
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
        .catch(error => console.error('Error fetching books:', error));
});
