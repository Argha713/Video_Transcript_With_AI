import whisper
import sys

model = whisper.load_model("base")

if len(sys.argv) < 2:
    print("Usage: python whisper_wrapper.py <audio_path>")
    sys.exit(1)

audio_path = sys.argv[1]
result = model.transcribe(audio_path)
print(result["text"])
