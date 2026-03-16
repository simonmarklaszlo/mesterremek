Please copy the Leaflet marker images from node_modules/leaflet/dist/images to this folder:

cp node_modules/leaflet/dist/images/* src/assets/leaflet/

On Windows (PowerShell):
Copy-Item -Path node_modules\leaflet\dist\images\* -Destination src\assets\leaflet\

These files are required for marker icons to display correctly:
- marker-icon.png
- marker-icon-2x.png
- marker-shadow.png
