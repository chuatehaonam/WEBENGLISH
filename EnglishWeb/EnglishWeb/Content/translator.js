// Word Translator Script - Tái sử dụng cho toàn website
// Double-click bất kỳ từ nào để dịch từ tiếng Anh sang tiếng Việt

document.addEventListener('DOMContentLoaded', function () {
    // Event listener cho double-click để chọn từ
    document.addEventListener('dblclick', function (e) {
        const selection = window.getSelection();
        const word = selection.toString().trim();

        if (word) {
            showPopup(word, e.pageX, e.pageY);
        }
    });

    // Đóng popup khi click ra ngoài
    document.addEventListener('click', function (e) {
        const popup = document.getElementById('popup-box');
        if (popup && !popup.contains(e.target)) {
            hidePopup();
        }
    });
});

function showPopup(word, x, y) {
    // Xóa popup cũ nếu có
    hidePopup();

    const popup = document.createElement('div');
    popup.id = 'popup-box';
    popup.style.position = 'absolute';
    popup.style.zIndex = 9999;
    popup.style.background = 'linear-gradient(145deg, #f8fbff, #ffffff)';
    popup.style.border = '2px solid #4a90e2';
    popup.style.padding = '8px';
    popup.style.borderRadius = '6px';
    popup.style.boxShadow = '0 3px 15px rgba(74, 144, 226, 0.2)';
    popup.style.maxWidth = '300px';
    popup.style.maxHeight = '350px';
    popup.style.overflowY = 'auto';
    popup.style.fontSize = '12px';

    popup.innerHTML = `
        <div style="margin-bottom: 6px; padding: 6px; background: linear-gradient(90deg, #e3f2fd, #bbdefb); border-radius: 4px;">
            <strong style="color: #1565c0;">📚 ${word}</strong>
        </div>
        <div id="translation-result" style="margin-top: 6px;"></div>
        <div id="dictionary-result" style="margin-top: 6px;"></div>
    `;

    document.body.appendChild(popup);

    // Điều chỉnh vị trí popup để không bị tràn màn hình
    const rect = popup.getBoundingClientRect();
    if (x + rect.width > window.innerWidth) {
        x = window.innerWidth - rect.width - 10;
    }
    if (y + rect.height > window.innerHeight) {
        y = window.innerHeight - rect.height - 10;
    }

    popup.style.left = `${Math.max(10, x)}px`;
    popup.style.top = `${Math.max(10, y)}px`;
    popup.style.display = 'block';

    // Tự động tra cứu từ
    translateWordAuto(word);
}

function hidePopup() {
    const popup = document.getElementById('popup-box');
    if (popup) {
        popup.remove();
    }
}

async function translateWordAuto(word) {
    const resultDiv = document.getElementById('translation-result');
    const dictDiv = document.getElementById('dictionary-result');

    if (!word) {
        resultDiv.innerHTML = '<div style="color: #1565c0; font-size: 11px;">Không có từ để tra cứu</div>';
        dictDiv.innerHTML = '';
        return;
    }

    // Hiển thị loading cho cả hai phần
    resultDiv.innerHTML = '<div style="color: #1976d2; font-size: 11px;">Đang dịch...</div>';
    dictDiv.innerHTML = '<div style="color: #1976d2; font-size: 11px;">Đang tra từ điển...</div>';

    // Chạy song song cả dịch thuật và từ điển
    const translationPromise = getTranslation(word);
    const dictionaryPromise = getBasicWordInfo(word);

    try {
        const [translation, dictData] = await Promise.all([translationPromise, dictionaryPromise]);

        // Hiển thị kết quả dịch
        if (translation) {
            resultDiv.innerHTML = `<div style="color: white; padding: 6px; background: linear-gradient(135deg, #2196f3, #1976d2); border-radius: 4px; margin-bottom: 6px; font-size: 11px; box-shadow: 0 2px 6px rgba(33, 150, 243, 0.3);">
                <strong>🔄 Bản dịch:</strong> ${translation}
            </div>`;
        } else {
            resultDiv.innerHTML = '<div style="color: #1565c0; font-size: 11px;">Không thể dịch từ này</div>';
        }

        // Hiển thị từ điển cơ bản
        if (dictData) {
            displayBasicWordInfo(dictData);
        } else {
            dictDiv.innerHTML = '<div style="color: #1565c0; font-style: italic; font-size: 11px;">Không tìm thấy thông tin từ điển</div>';
        }

    } catch (error) {
        console.error('Error:', error);
        resultDiv.innerHTML = '<div style="color: #1565c0; font-size: 11px;"><strong>Lỗi:</strong> Không thể kết nối đến internet.</div>';
        dictDiv.innerHTML = '';
    }
}

async function getTranslation(word) {
    try {
        const response = await fetch(`https://api.mymemory.translated.net/get?q=${encodeURIComponent(word)}&langpair=en|vi&de=your@email.com`, {
            method: 'GET',
        });

        if (!response.ok) throw new Error('Translation failed');

        const data = await response.json();
        return data.responseData.translatedText;
    } catch (error) {
        console.error('Translation error:', error);
        return null;
    }
}

async function getBasicWordInfo(word) {
    try {
        const response = await fetch(`https://api.dictionaryapi.dev/api/v2/entries/en/${word}`);

        if (!response.ok) throw new Error('Dictionary lookup failed');

        const data = await response.json();
        return data && data.length > 0 ? data[0] : null;
    } catch (error) {
        console.error('Dictionary error:', error);
        return null;
    }
}

function displayBasicWordInfo(wordData) {
    const resultDiv = document.getElementById('dictionary-result');
    let html = '';

    // Phiên âm (không có nút phát âm)
    if (wordData.phonetics && wordData.phonetics.length > 0) {
        const phonetic = wordData.phonetics.find(p => p.text) || wordData.phonetics[0];
        if (phonetic.text) {
            html += `<div style="margin-bottom: 6px; padding: 4px; background: linear-gradient(135deg, #e3f2fd, #bbdefb); border-radius: 4px; font-size: 11px;">
                        <strong style="color: #1565c0;">📢</strong> ${phonetic.text}
                     </div>`;
        }
    }

    // Nghĩa gọn gàng (chỉ 1-2 nghĩa chính)
    if (wordData.meanings && wordData.meanings.length > 0) {
        wordData.meanings.slice(0, 2).forEach((meaning) => {
            html += `<div style="margin-bottom: 4px; padding: 4px; background: linear-gradient(135deg, #f3f9ff, #e1f5fe); border-radius: 4px;">
                        <strong style="color: #1976d2;">🏷️ ${meaning.partOfSpeech}</strong>`;

            if (meaning.definitions && meaning.definitions.length > 0) {
                const def = meaning.definitions[0].definition;
                html += `<div style="margin-top: 2px; font-size: 11px; color: #0277bd;">${def}</div>`;
            }

            // Từ đồng nghĩa (rất gọn)
            if (meaning.synonyms && meaning.synonyms.length > 0) {
                html += `<div style="margin-top: 3px; font-size: 10px;">
                            <span style="color: #29b6f6;">🔄 ${meaning.synonyms.slice(0, 3).join(', ')}</span>
                         </div>`;
            }

            html += '</div>';
        });
    }

    resultDiv.innerHTML = html || '<div style="color: #1565c0; font-style: italic; font-size: 11px;">Không có thông tin từ điển</div>';
}

function lookupWord(word) {
    // Cập nhật header với từ mới
    const headerDiv = document.querySelector('#popup-box div');
    if (headerDiv) {
        headerDiv.innerHTML = `<strong style="color: #1565c0;">📚 ${word}</strong>`;
    }
    // Tự động tra từ mới
    translateWordAuto(word);
} 