openssl req -nodes -new -x509 -keyout mmo.key -out mmo.cer
openssl pkcs12 -nodes -export -out mmo.pfx -inkey mmo.key -in mmo.cer
cp mmo.cer ../MMORPGClient/Assets/StreamingAssets/
cp mmo.pfx ../MMORPGServer/
