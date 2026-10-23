from gtts import gTTS

# テキストを音声に変換
language = input("言語を選択(日本語ならja, 英語ならen): ")
text = input("出力する音声のテキストを入力: ")
tts = gTTS(text, lang = language)

# 音声ファイルとして保存
tts.save(f"../../Assets/Sound/TTS/{text}.mp3")
print(f"音声ファイルが{text}.mp3として保存されました。")