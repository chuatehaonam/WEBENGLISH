// Word Translator Script - Tái sử dụng cho toàn website
// Double-click bất kỳ từ nào để dịch từ tiếng Anh sang tiếng Việt

document.addEventListener('DOMContentLoaded', function() {
    // Event listener cho double-click để chọn từ
    document.addEventListener('dblclick', function (e) {
        const selection = window.getSelection();
        const word = selection.toString().trim();

        if (word) {
            showPopup(word, e.pageX, e.pageY);
        }
    });

    // Đóng popup khi click ra ngoài
    document.addEventListener('click', function(e) {
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
    popup.style.background = '#fff';
    popup.style.border = '1px solid #ccc';
    popup.style.padding = '10px';
    popup.style.borderRadius = '8px';
    popup.style.boxShadow = '0 2px 6px rgba(0,0,0,0.15)';
    popup.style.maxWidth = '400px';
    popup.style.maxHeight = '600px';
    popup.style.overflowY = 'auto';
    popup.style.fontSize = '14px';
    
    popup.innerHTML = `
        <div style="margin-bottom: 8px;">
            <strong>Từ được chọn:</strong> ${word}
        </div>
        <div style="margin-bottom: 8px;">
            <input type="text" value="${word}" id="edit-word" style="width: 100%; padding: 4px; border: 1px solid #ddd; border-radius: 4px;" />
        </div>
        <div style="margin-bottom: 8px;">
            <button onclick="translateWord()" style="background: #007bff; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; margin-right: 5px;" title="Dịch và tra từ điển">📝 Dịch & Từ điển</button>
            <button onclick="hidePopup()" style="background: #6c757d; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer;">❌ Đóng</button>
        </div>
        <div id="translation-result" style="margin-top: 10px;"></div>
        <div id="dictionary-result" style="margin-top: 10px;"></div>
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

    // Focus vào input
    document.getElementById('edit-word').focus();
    
    // Enter key để translate
    document.getElementById('edit-word').addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            translateWord();
        }
    });
}

function hidePopup() {
    const popup = document.getElementById('popup-box');
    if (popup) {
        popup.remove();
    }
}

async function translateWord() {
    const word = document.getElementById('edit-word').value.trim();
    const resultDiv = document.getElementById('translation-result');
    const dictDiv = document.getElementById('dictionary-result');
    
    if (!word) {
        resultDiv.innerHTML = '<div style="color: red;">Vui lòng nhập từ cần dịch</div>';
        dictDiv.innerHTML = '';
        return;
    }
    
    // Hiển thị loading cho cả hai phần
    resultDiv.innerHTML = '<div style="color: #007bff;">Đang dịch...</div>';
    dictDiv.innerHTML = '<div style="color: #007bff;">Đang tra từ điển...</div>';
    
    // Chạy song song cả dịch thuật và từ điển
    const translationPromise = getTranslation(word);
    const dictionaryPromise = getBasicWordInfo(word);
    
    try {
        const [translation, dictData] = await Promise.all([translationPromise, dictionaryPromise]);
        
        // Hiển thị kết quả dịch
        if (translation) {
            resultDiv.innerHTML = `<div style="color: #28a745; padding: 8px; background: #f8f9fa; border-radius: 4px; margin-bottom: 10px;">
                <strong>🔄 Bản dịch:</strong> ${translation}
            </div>`;
        } else {
            resultDiv.innerHTML = '<div style="color: red;">Không thể dịch từ này</div>';
        }
        
        // Hiển thị từ điển cơ bản
        if (dictData) {
            displayBasicWordInfo(dictData);
        } else {
            dictDiv.innerHTML = '<div style="color: #666; font-style: italic;">Không tìm thấy thông tin từ điển</div>';
        }
        
    } catch (error) {
        console.error('Error:', error);
        resultDiv.innerHTML = '<div style="color: red;"><strong>Lỗi:</strong> Không thể kết nối đến internet.</div>';
        dictDiv.innerHTML = '';
    }
}

async function getTranslation(word) {
    try {
        const response = await fetch(`https://api.mymemory.translated.net/get?q=${encodeURIComponent(word)}&langpair=en|vi&de=translator@example.com`, {
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
    
    // Phát âm (ngắn gọn)
    if (wordData.phonetics && wordData.phonetics.length > 0) {
        const phonetic = wordData.phonetics.find(p => p.text) || wordData.phonetics[0];
        if (phonetic.text) {
            html += `<div style="margin-bottom: 8px; padding: 6px; background: #e3f2fd; border-radius: 4px; font-size: 13px;">
                        <strong>📢</strong> ${phonetic.text}`;
            if (phonetic.audio) {
                html += ` <button onclick="playAudio('${phonetic.audio}')" style="background: #2196f3; color: white; border: none; padding: 1px 4px; border-radius: 2px; cursor: pointer; font-size: 11px;">🔊</button>`;
            }
            html += `</div>`;
        }
    }
    
    // Nghĩa gọn gàng (chỉ 1-2 nghĩa chính)
    if (wordData.meanings && wordData.meanings.length > 0) {
        wordData.meanings.slice(0, 2).forEach((meaning) => {
            html += `<div style="margin-bottom: 6px; padding: 6px; background: #f5f5f5; border-radius: 4px;">
                        <strong style="color: #1976d2;">🏷️ ${meaning.partOfSpeech}</strong>`;
            
            if (meaning.definitions && meaning.definitions.length > 0) {
                const def = meaning.definitions[0].definition;
                html += `<div style="margin-top: 2px; font-size: 13px;">${def}</div>`;
            }
            
            // Từ đồng nghĩa (rất gọn)
            if (meaning.synonyms && meaning.synonyms.length > 0) {
                html += `<div style="margin-top: 4px; font-size: 12px;">
                            <span style="color: #4caf50;">🔄 Đồng nghĩa: ${meaning.synonyms.slice(0, 3).join(', ')}</span>
                         </div>`;
            }
            
            // Từ trái nghĩa (rất gọn)
            if (meaning.antonyms && meaning.antonyms.length > 0) {
                html += `<div style="margin-top: 4px; font-size: 12px;">
                            <span style="color: #f44336;">↔️ Trái nghĩa: ${meaning.antonyms.slice(0, 3).join(', ')}</span>
                         </div>`;
            }
            
            html += '</div>';
        });
    }
    
    resultDiv.innerHTML = html || '<div style="color: #666; font-style: italic;">Không có thông tin từ điển</div>';
}





// Hàm phát âm thanh
function playAudio(audioUrl) {
    if (!audioUrl) return;
    
    try {
        const audio = new Audio(audioUrl);
        audio.play().catch(error => {
            console.error('Không thể phát âm thanh:', error);
            // Thử với URL khác nếu có thể
            if (audioUrl.startsWith('//')) {
                const httpsUrl = 'https:' + audioUrl;
                const httpsAudio = new Audio(httpsUrl);
                httpsAudio.play().catch(err => {
                    console.error('Vẫn không thể phát âm thanh:', err);
                });
            }
        });
    } catch (error) {
        console.error('Lỗi tạo audio:', error);
    }
}

// Thêm CSS styles động cho popup
if (!document.getElementById('translator-styles')) {
    const style = document.createElement('style');
    style.id = 'translator-styles';
    style.innerHTML = `
        #popup-box {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.4;
        }
        
        #popup-box button:hover {
            opacity: 0.8;
            transform: translateY(-1px);
        }
        
        #popup-box input:focus {
            outline: none;
            border-color: #007bff;
            box-shadow: 0 0 0 2px rgba(0,123,255,0.25);
        }
        
        #popup-box ul {
            list-style-type: disc;
        }
        
        #popup-box li {
            margin-bottom: 2px;
        }
        
        /* Scrollbar styling */
        #popup-box::-webkit-scrollbar {
            width: 6px;
        }
        
        #popup-box::-webkit-scrollbar-track {
            background: #f1f1f1;
            border-radius: 3px;
        }
        
        #popup-box::-webkit-scrollbar-thumb {
            background: #c1c1c1;
            border-radius: 3px;
        }
        
        #popup-box::-webkit-scrollbar-thumb:hover {
            background: #a8a8a8;
        }
    `;
    document.head.appendChild(style);
}