import json
import os

def convert_code_to_json(file_path, language="csharp"):
    """
    讀取程式碼檔案並轉換為 JSON 格式字串
    """
    if not os.path.exists(file_path):
        return f"錯誤：找不到檔案 {file_path}"

    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            raw_code = f.read()

        # 建立物件結構
        code_obj = {
            "code_sample": {
                "language": language,
                "code": raw_code
            }
        }

        # json.dumps 會自動處理 \n (換行) 與 \" (雙引號轉義)
        # indent=4 讓輸出的 JSON 檔案易於閱讀
        # ensure_ascii=False 確保中文註解不會變成 Unicode 碼 (\uXXXX)
        json_output = json.dumps(code_obj, ensure_ascii=False, indent=4)
        
        return json_output

    except Exception as e:
        return f"處理時發生錯誤: {str(e)}"

file_name = "SpaceCatScripts.cs"
result_json = convert_code_to_json(file_name)

print(result_json)