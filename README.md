# Face recognition based metadata suggestions for M-Files

_This solution is for demo purposes only and is not meant to be used in production._



The solution consists of two applications: 
- Vault application (SmartFaceVaultApp) 
   - Encodes face in new pictures when saved to the M-Files vault.
   - Saves the face encoding to a property for future face comparison.
- Intelligence service application (SmartFace)
   - Encodes faces in pictures while saving to the M-Files vault.
   - Searches matching faces from the vault.
   - Suggests objects with references to matching face encodings as metadata while a document (picture) is saved to the vault.

Face recognition is implemented by [FaceRecognitionDotNet](https://github.com/takuya-takeuchi/FaceRecognitionDotNet) library.


To use the applications you need to download face recognition models from https://github.com/ageitgey/face_recognition_models to somewhere where your M-Files Server can access them.


