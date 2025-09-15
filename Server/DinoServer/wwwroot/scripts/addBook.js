document.getElementById('addBookForm').addEventListener('submit', function(event) {
    event.preventDefault();

    const userId = document.getElementById('userId').value;
    const formData = new FormData(event.target);
    const book = {
        title: formData.get('title'),
        author: formData.get('author'),
        genres: formData.get('genres').split(',').map(genre => genre.trim()),
        publicationYear: parseInt(formData.get('publicationYear')),
        annotation: formData.get('annotation'),
        isbn: formData.get('isbn')
    };

    let isValid = true;

    // Валидация полей
    // 'valid' 'invalid' подсвечивают невалидные поля 
    if (!validateBookTitle(book.title)) {
        document.getElementById('title').classList.add('invalid');
        document.getElementById('title').classList.remove('valid');
        document.getElementById('titleError').textContent = 'Title is required.';
        isValid = false;
    } else {
        document.getElementById('title').classList.remove('invalid');
        document.getElementById('title').classList.add('valid');
        document.getElementById('titleError').textContent = '';
    }

    if (!validateAuthorName(book.author)) {
        document.getElementById('author').classList.add('invalid');
        document.getElementById('author').classList.remove('valid');
        document.getElementById('authorError').textContent = 'Author is required.';
        isValid = false;
    } else {
        document.getElementById('author').classList.remove('invalid');
        document.getElementById('author').classList.add('valid');
        document.getElementById('authorError').textContent = '';
    }

    let genres;
    if (!validateGenres(book.genres.join(','), genres)) {
        document.getElementById('genres').classList.add('invalid');
        document.getElementById('genres').classList.remove('valid');
        document.getElementById('genresError').textContent = 'Genres are required.';
        isValid = false;
    } else {
        document.getElementById('genres').classList.remove('invalid');
        document.getElementById('genres').classList.add('valid');
        document.getElementById('genresError').textContent = '';
    }

    if (isNaN(book.publicationYear) || book.publicationYear <= 0) {
        document.getElementById('publicationYear').classList.add('invalid');
        document.getElementById('publicationYear').classList.remove('valid');
        document.getElementById('publicationYearError').textContent = 'Publication year is required and must be a positive number.';
        isValid = false;
    } else {
        document.getElementById('publicationYear').classList.remove('invalid');
        document.getElementById('publicationYear').classList.add('valid');
        document.getElementById('publicationYearError').textContent = '';
    }

    if (!validateAnnotation(book.annotation)) {
        document.getElementById('annotation').classList.add('invalid');
        document.getElementById('annotation').classList.remove('valid');
        document.getElementById('annotationError').textContent = 'Annotation is required.';
        isValid = false;
    } else {
        document.getElementById('annotation').classList.remove('invalid');
        document.getElementById('annotation').classList.add('valid');
        document.getElementById('annotationError').textContent = '';
    }

    if (!validateISBN(book.isbn)) {
        document.getElementById('isbn').classList.add('invalid');
        document.getElementById('isbn').classList.remove('valid');
        document.getElementById('isbnError').textContent = 'ISBN is invalid.';
        isValid = false;
    } else {
        document.getElementById('isbn').classList.remove('invalid');
        document.getElementById('isbn').classList.add('valid');
        document.getElementById('isbnError').textContent = '';
    }

    let userIdValid;
    if (!validateUserId(userId.toString(), userIdValid)) {
        document.getElementById('userId').classList.add('invalid');
        document.getElementById('userId').classList.remove('valid');
        document.getElementById('userIdError').textContent = 'User ID is required and must be a positive number.';
        isValid = false;
    } else {
        document.getElementById('userId').classList.remove('invalid');
        document.getElementById('userId').classList.add('valid');
        document.getElementById('userIdError').textContent = '';
    }

    if (isValid) {
        fetch(`/api/book/addbook?userId=${userId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(book)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                alert('Book added successfully!');
                event.target.reset(); // Очистить форму после успешного добавления
            })
            .catch(error => console.error('Error adding book:', error));
    }
});

// Валидационные функции
function validateBookTitle(title) {
    return !!(title && title.trim());
}

function validateAuthorName(author) {
    return !!(author && author.trim());
}

function validateGenres(genresInput, genres) {
    genres = genresInput.split(',').map(item => item.trim()).filter(item => item);
    return genres.length > 0;
}

function validatePublicationYear(year) {
    return !isNaN(year) && year > 0;
}

function validateAnnotation(annotation) {
    return !!(annotation && annotation.trim());
}

function validateISBN(isbn) {
    if (!isbn || !isbn.trim()) return false;

    isbn = isbn.replace(/[- ]/g, ''); // Убираем дефисы и пробелы

    if (isbn.length === 10) {
        return validateISBN10(isbn);
    } else if (isbn.length === 13) {
        return validateISBN13(isbn);
    }

    return false; // ISBN не соответствует допустимым длинам
}

function validateISBN10(isbn) {
    if (!/^\d{9}[\dX]$/.test(isbn)) return false;

    let sum = 0;
    for (let i = 0; i < 9; i++) {
        sum += (isbn[i] - '0') * (10 - i);
    }

    const checkDigit = isbn[9];
    sum += (checkDigit === 'X') ? 10 : (checkDigit - '0');

    return sum % 11 === 0;
}

function validateISBN13(isbn) {
    if (!/^\d{13}$/.test(isbn)) return false;

    let sum = 0;
    for (let i = 0; i < 12; i++) {
        const digit = isbn[i] - '0';
        sum += (i % 2 === 0) ? digit : digit * 3;
    }

    const checkDigit = 10 - (sum % 10);
    return isbn[12] - '0' === checkDigit;
}

function validateUserId(input, userId) {
    return !isNaN(input) && parseInt(input) > 0;
}
