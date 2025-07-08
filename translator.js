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
            <button onclick="translateWord()" style="background: #007bff; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; margin-right: 5px;" title="Dịch từ">📝 Dịch</button>
            <button onclick="hidePopup()" style="background: #6c757d; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer;">❌ Đóng</button>
        </div>
        <div id="translation-result" style="margin-top: 10px;"></div>
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
    
    if (!word) {
        resultDiv.innerHTML = '<div style="color: red;">Vui lòng nhập từ cần dịch</div>';
        return;
    }
    
    // Hiển thị loading
    resultDiv.innerHTML = '<div style="color: #007bff;">Đang dịch...</div>';
    
    try {
        const translation = await getTranslation(word);
        
        // Hiển thị kết quả dịch
        if (translation) {
            if (translation.startsWith('ERROR:')) {
                resultDiv.innerHTML = `<div style="color: #dc3545; padding: 8px; background: #f8d7da; border: 1px solid #f5c6cb; border-radius: 4px; margin-bottom: 10px;">
                    <strong>⚠️ Lỗi:</strong> ${translation.replace('ERROR: ', '')}
                </div>`;
            } else {
                resultDiv.innerHTML = `<div style="color: #28a745; padding: 8px; background: #f8f9fa; border-radius: 4px; margin-bottom: 10px;">
                    <strong>🔄 Bản dịch:</strong> ${translation}
                </div>`;
            }
        } else {
            resultDiv.innerHTML = '<div style="color: red;">Không thể dịch từ này</div>';
        }
        
    } catch (error) {
        console.error('Error:', error);
        resultDiv.innerHTML = '<div style="color: red;"><strong>Lỗi:</strong> Không thể kết nối đến service dịch thuật.</div>';
    }
}

async function getTranslation(word) {
    try {
        const response = await fetch('http://localhost:5000/translate', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                text: word
            })
        });
        
        if (!response.ok) throw new Error('Translation service không phản hồi');
        
        const data = await response.json();
        
        if (data.status === 'success') {
            return data.translated_text;
        } else {
            throw new Error(data.error || 'Model dịch thuật gặp lỗi');
        }
    } catch (error) {
        console.error('Translation error:', error);
        
        // Trả về thông báo lỗi rõ ràng
        if (error.message.includes('fetch')) {
            return 'ERROR: Service dịch thuật không hoạt động. Vui lòng khởi động translation_service.py';
        } else {
            return `ERROR: ${error.message}`;
        }
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