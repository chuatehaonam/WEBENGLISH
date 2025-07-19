// Simple Word Translator Script - Double-click để dịch từ
// Hỗ trợ: Anh-Việt, Việt-Anh, Trung-Việt, Nhật-Việt, Hàn-Việt

document.addEventListener('DOMContentLoaded', function () {
    // Event listener cho double-click để chọn từ
    document.addEventListener('dblclick', function (e) {        
        const selection = window.getSelection();
        const word = selection.toString().trim();

        if (word && word.length > 0) {
            // Ngăn default selection behavior
            e.preventDefault();
            
            // Lấy vị trí chính xác của mouse click
            const clickX = e.clientX + window.pageXOffset;
            const clickY = e.clientY + window.pageYOffset;
            
            showPopup(word, clickX, clickY);
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
        <div style="margin-bottom: 8px; padding: 8px; background: linear-gradient(90deg, #e3f2fd, #bbdefb); border-radius: 6px; display: flex; justify-content: space-between; align-items: center;">
            <strong style="color: #1565c0;">📚 ${word}</strong>
            <button onclick="hidePopup()" style="background: none; border: 1px solid #f44336; color: #f44336; font-size: 10px; padding: 2px 6px; border-radius: 3px; cursor: pointer;" title="Đóng">✕</button>
        </div>
        <div id="language-detection" style="margin-bottom: 6px; font-size: 10px; color: #666; font-style: italic;"></div>
        <div id="translation-result" style="margin-top: 6px;"></div>
        <div id="dictionary-result" style="margin-top: 6px;"></div>
        <div style="margin-top: 8px; padding: 4px; background: #f5f5f5; border-radius: 4px; font-size: 9px; color: #666;">
            💡 Tip: Double-click từ khác để tra cứu tiếp
        </div>
    `;

    document.body.appendChild(popup);

    // Tính toán vị trí để popup xuất hiện ngay tại chỗ double-click
    popup.style.left = `${x}px`;
    popup.style.top = `${y}px`;
    popup.style.display = 'block';

    // Sau khi hiển thị, điều chỉnh nếu bị tràn màn hình
    const rect = popup.getBoundingClientRect();
    let adjustedX = x;
    let adjustedY = y;

    // Kiểm tra tràn bên phải
    if (rect.right > window.innerWidth) {
        adjustedX = window.innerWidth - rect.width - 10;
    }
    
    // Kiểm tra tràn bên dưới
    if (rect.bottom > window.innerHeight) {
        adjustedY = y - rect.height - 10; // Hiển thị phía trên thay vì dưới
    }
    
    // Kiểm tra tràn bên trái
    if (adjustedX < 10) {
        adjustedX = 10;
    }
    
    // Kiểm tra tràn bên trên
    if (adjustedY < 10) {
        adjustedY = y + 20; // Hiển thị dưới cursor
    }

    popup.style.left = `${adjustedX}px`;
    popup.style.top = `${adjustedY}px`;

    // Tự động tra cứu từ
    translateWordAuto(word);
}

function hidePopup() {
    const popup = document.getElementById('popup-box');
    if (popup) {
        popup.remove();
    }
}

// Function phát hiện ngôn ngữ
function detectLanguage(text) {
    // Regex patterns cho các ngôn ngữ
    const patterns = {
        vietnamese: /[àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ]/i,
        english: /^[a-zA-Z\s'-]+$/,
        chinese: /[\u4e00-\u9fff]/,
        japanese: /[\u3040-\u309f\u30a0-\u30ff\u4e00-\u9fff]/,
        korean: /[\uac00-\ud7af]/
    };
    
    if (patterns.vietnamese.test(text)) return 'vi';
    if (patterns.chinese.test(text)) return 'zh';
    if (patterns.japanese.test(text)) return 'ja';
    if (patterns.korean.test(text)) return 'ko';
    if (patterns.english.test(text)) return 'en';
    
    return 'auto'; // Default
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
    resultDiv.innerHTML = '<div style="color: #1976d2; font-size: 11px;">🔄 Đang dịch...</div>';
    dictDiv.innerHTML = '<div style="color: #1976d2; font-size: 11px;">📖 Đang tra từ điển...</div>';

    // Chạy song song cả dịch thuật và từ điển
    const translationPromise = getTranslation(word);
    const dictionaryPromise = getBasicWordInfo(word);

    try {
        const [translationResult, dictData] = await Promise.all([translationPromise, dictionaryPromise]);

        // Hiển thị kết quả dịch
        if (translationResult && translationResult.translation) {
            const langNames = {
                'vi': 'Việt',
                'en': 'Anh', 
                'zh': 'Trung',
                'ja': 'Nhật',
                'ko': 'Hàn'
            };
            
            const sourceFlag = translationResult.sourceLang === 'vi' ? '🇻🇳' : 
                             translationResult.sourceLang === 'en' ? '🇺🇸' :
                             translationResult.sourceLang === 'zh' ? '🇨🇳' :
                             translationResult.sourceLang === 'ja' ? '🇯🇵' :
                             translationResult.sourceLang === 'ko' ? '🇰🇷' : '🌐';
                             
            const targetFlag = translationResult.targetLang === 'vi' ? '🇻🇳' : 
                             translationResult.targetLang === 'en' ? '🇺🇸' :
                             translationResult.targetLang === 'zh' ? '🇨🇳' :
                             translationResult.targetLang === 'ja' ? '🇯🇵' :
                             translationResult.targetLang === 'ko' ? '🇰🇷' : '🌐';
            
            resultDiv.innerHTML = `
                <div style="color: white; padding: 8px; background: linear-gradient(135deg, #2196f3, #1976d2); border-radius: 6px; margin-bottom: 8px; font-size: 12px; box-shadow: 0 3px 8px rgba(33, 150, 243, 0.3);">
                    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 4px;">
                        <strong>🔄 Bản dịch</strong>
                        <span style="font-size: 10px; opacity: 0.9;">${sourceFlag} ${langNames[translationResult.sourceLang]} → ${targetFlag} ${langNames[translationResult.targetLang]}</span>
                    </div>
                    <div style="background: rgba(255,255,255,0.15); padding: 6px; border-radius: 4px; font-weight: 500;">
                        ${translationResult.translation}
                    </div>
                </div>`;
        } else {
            resultDiv.innerHTML = '<div style="color: #1565c0; font-size: 11px;">❌ Không thể dịch từ này</div>';
        }

        // Hiển thị từ điển cơ bản (chỉ cho tiếng Anh)
        if (dictData && detectLanguage(word) === 'en') {
            displayBasicWordInfo(dictData);
        } else {
            dictDiv.innerHTML = '<div style="color: #1565c0; font-style: italic; font-size: 11px;">ℹ️ Từ điển chỉ hỗ trợ tiếng Anh</div>';
        }

    } catch (error) {
        console.error('Error:', error);
        resultDiv.innerHTML = '<div style="color: #e74c3c; font-size: 11px;"><strong>⚠️ Lỗi:</strong> Không thể kết nối đến internet.</div>';
        dictDiv.innerHTML = '';
    }
}

async function getTranslation(word) {
    try {
        // Phát hiện ngôn ngữ đầu vào
        const detectedLang = detectLanguage(word);
        let sourceLang, targetLang;
        
        // Cập nhật thông tin phát hiện ngôn ngữ
        const detectionDiv = document.getElementById('language-detection');
        if (detectionDiv) {
            const langNames = {
                'vi': 'Tiếng Việt',
                'en': 'Tiếng Anh', 
                'zh': 'Tiếng Trung',
                'ja': 'Tiếng Nhật',
                'ko': 'Tiếng Hàn',
                'auto': 'Tự động phát hiện'
            };
            detectionDiv.textContent = `🔍 Phát hiện: ${langNames[detectedLang] || 'Không xác định'}`;
        }
        
        // Xác định hướng dịch tự động
        sourceLang = detectedLang === 'vi' ? 'vi' : (detectedLang || 'en');
        targetLang = sourceLang === 'vi' ? 'en' : 'vi';
        
        const langPair = `${sourceLang}|${targetLang}`;
        
        const response = await fetch(`https://api.mymemory.translated.net/get?q=${encodeURIComponent(word)}&langpair=${langPair}&de=your@email.com`, {
            method: 'GET',
        });

        if (!response.ok) throw new Error('Translation failed');

        const data = await response.json();
        return {
            translation: data.responseData.translatedText,
            sourceLang: sourceLang,
            targetLang: targetLang
        };
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