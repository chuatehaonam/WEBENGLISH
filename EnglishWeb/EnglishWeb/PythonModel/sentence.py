from flask import Flask, request, jsonify
from sentence_transformers import SentenceTransformer, util
import re

app = Flask(__name__)
model = SentenceTransformer('distiluse-base-multilingual-cased-v2')

@app.route('/similarity', methods=['POST'])
def get_similarity():
    data = request.json
    sentence1 = data['sentence1'].strip()  # originalText
    sentence2 = data['sentence2'].strip()  # userTranslation
    
    # Kiểm tra độ dài câu
    len1 = len(sentence1.split())
    len2 = len(sentence2.split())
    
    # Nếu bản dịch quá ngắn hoặc quá dài so với gốc, trừ điểm
    length_ratio = min(len1, len2) / max(len1, len2) if max(len1, len2) > 0 else 0
    length_penalty = 1.0
    
    if length_ratio < 0.5:  # Chênh lệch độ dài quá nhiều
        length_penalty = 0.7
    elif length_ratio < 0.7:
        length_penalty = 0.85
    
    # Encode cả hai câu
    vec1 = model.encode(sentence1, convert_to_tensor=True)
    vec2 = model.encode(sentence2, convert_to_tensor=True)
    
    # Tính similarity
    similarity = util.pytorch_cos_sim(vec1, vec2).item()
    
    # Áp dụng công thức chấm điểm nghiêm khắc hơn
    if similarity < 0.3:
        score = 0
    elif similarity < 0.5:
        score = similarity * 2  # 0.6-1.0
    elif similarity < 0.7:
        score = 2 + (similarity - 0.5) * 10  # 2.0-4.0  
    elif similarity < 0.85:
        score = 4 + (similarity - 0.7) * 20  # 4.0-7.0
    else:
        score = 7 + (similarity - 0.85) * 20  # 7.0-10.0
    
    # Áp dụng penalty về độ dài
    final_score = score * length_penalty
    
    # Đảm bảo điểm trong khoảng 0-10
    final_score = max(0, min(10, final_score))

    return jsonify({
        'score': round(final_score, 1),
        'raw_similarity': round(similarity, 3),
        'length_penalty': round(length_penalty, 2)
    })

if __name__ == '__main__':
    app.run(port=5000)