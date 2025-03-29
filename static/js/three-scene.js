class CityScene {
    constructor(game) {
        this.game = game;
        this.scene = null;
        this.camera = null;
        this.renderer = null;
        this.controls = null;
        this.ground = null;
        this.buildings = new Map();
        this.buildingPreview = null;
        this.raycaster = new THREE.Raycaster();
        this.mouse = new THREE.Vector2();
        this.gridHelper = null;
        this.GRID_SIZE = 50;
        this.CELL_SIZE = 10;
        this.terrain = null;
        this.terrainGeometry = null;
        this.waterMesh = null;
        this.mapCenter = { lat: 45.52592187, lng: 158.1083605 };
        this.mapZoom = 12;
        this.mapboxToken = null;
        this.terrainData = null;
        this.terrainSize = 500;
    }

    setMapboxToken(token) {
        this.mapboxToken = token;
    }

    async updateLocation(lat, lng) {
        this.mapCenter = { lat, lng };
        await this.createMapTerrain();
    }

    init() {
        // Create scene with fog for depth
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0x87ceeb);
        this.scene.fog = new THREE.FogExp2(0x87ceeb, 0.002);

        // Create camera with better initial position
        this.camera = new THREE.PerspectiveCamera(
            60,
            window.innerWidth / window.innerHeight,
            0.1,
            2000
        );
        this.camera.position.set(200, 200, 200);
        this.camera.lookAt(0, 0, 0);

        // Create renderer with better shadows
        this.renderer = new THREE.WebGLRenderer({ 
            antialias: true,
            logarithmicDepthBuffer: true
        });
        this.renderer.setSize(window.innerWidth, window.innerHeight);
        this.renderer.shadowMap.enabled = true;
        this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
        document.getElementById('three-container').appendChild(this.renderer.domElement);

        // Add orbit controls with better constraints
        this.controls = new THREE.OrbitControls(this.camera, this.renderer.domElement);
        this.controls.enableDamping = true;
        this.controls.dampingFactor = 0.05;
        this.controls.screenSpacePanning = false;
        this.controls.minDistance = 50;
        this.controls.maxDistance = 500;
        this.controls.maxPolarAngle = Math.PI / 2.1;
        this.controls.minPolarAngle = 0.1;

        // Add better lighting
        this.setupLighting();

        // Create enhanced terrain
        this.createTerrain();

        // Add water
        this.createWater();

        // Add grid helper that follows terrain
        this.createGridHelper();

        // Add event listeners
        window.addEventListener('resize', () => this.onWindowResize());
        this.renderer.domElement.addEventListener('mousemove', (e) => this.onMouseMove(e));
        this.renderer.domElement.addEventListener('click', (e) => this.onMouseClick(e));

        // Start animation loop
        this.animate();

        console.log('3D scene initialized');
    }

    setupLighting() {
        // Ambient light for general illumination
        const ambientLight = new THREE.AmbientLight(0xffffff, 0.4);
        this.scene.add(ambientLight);

        // Main directional light (sun)
        const sunLight = new THREE.DirectionalLight(0xffffff, 1);
        sunLight.position.set(100, 100, 50);
        sunLight.castShadow = true;
        sunLight.shadow.mapSize.width = 2048;
        sunLight.shadow.mapSize.height = 2048;
        sunLight.shadow.camera.near = 0.5;
        sunLight.shadow.camera.far = 500;
        sunLight.shadow.camera.left = -200;
        sunLight.shadow.camera.right = 200;
        sunLight.shadow.camera.top = 200;
        sunLight.shadow.camera.bottom = -200;
        this.scene.add(sunLight);

        // Secondary light for better shadows
        const fillLight = new THREE.DirectionalLight(0x8088ff, 0.3);
        fillLight.position.set(-100, 50, -50);
        this.scene.add(fillLight);
    }

    createTerrain() {
        const size = this.GRID_SIZE * this.CELL_SIZE;
        const resolution = 128;
        const geometry = new THREE.PlaneGeometry(size, size, resolution, resolution);
        
        // Generate heightmap
        const vertices = geometry.attributes.position.array;
        for (let i = 0; i < vertices.length; i += 3) {
            const x = vertices[i] / size;
            const z = vertices[i + 2] / size;
            
            // Combine multiple noise functions for more natural terrain
            const elevation = 
                this.noise(x * 2, z * 2) * 15 +
                this.noise(x * 4, z * 4) * 7.5 +
                this.noise(x * 8, z * 8) * 3.75;
            
            vertices[i + 1] = Math.max(0, elevation);
        }
        geometry.computeVertexNormals();

        // Create terrain material with texture blending
        const terrainMaterial = new THREE.MeshStandardMaterial({
            color: 0x7ec850,
            roughness: 0.8,
            metalness: 0.1,
            vertexColors: true
        });

        // Add vertex colors based on height and slope
        const colors = new Float32Array(vertices.length);
        for (let i = 0; i < vertices.length; i += 3) {
            const height = vertices[i + 1];
            const normal = new THREE.Vector3(
                geometry.attributes.normal.array[i],
                geometry.attributes.normal.array[i + 1],
                geometry.attributes.normal.array[i + 2]
            );
            
            // Calculate colors based on height and slope
            const slope = 1 - normal.dot(new THREE.Vector3(0, 1, 0));
            let color = new THREE.Color();
            
            if (height < 1) {
                color.setHex(0x7ec850); // Grass
            } else if (height < 5) {
                color.setHex(0x6db344); // Dark grass
            } else if (height < 10) {
                color.setHex(0x8b6914); // Dirt
            } else {
                color.setHex(0x808080); // Rock
            }
            
            // Adjust color based on slope
            color.multiplyScalar(1 - slope * 0.5);
            
            colors[i] = color.r;
            colors[i + 1] = color.g;
            colors[i + 2] = color.b;
        }
        geometry.setAttribute('color', new THREE.BufferAttribute(colors, 3));

        this.terrain = new THREE.Mesh(geometry, terrainMaterial);
        this.terrain.rotation.x = -Math.PI / 2;
        this.terrain.receiveShadow = true;
        this.scene.add(this.terrain);
    }

    createWater() {
        const size = this.GRID_SIZE * this.CELL_SIZE * 1.5;
        const waterGeometry = new THREE.PlaneGeometry(size, size);
        const waterMaterial = new THREE.MeshPhysicalMaterial({
            color: 0x0077be,
            transparent: true,
            opacity: 0.6,
            roughness: 0,
            metalness: 0,
            clearcoat: 1.0,
            clearcoatRoughness: 0.1,
        });

        this.waterMesh = new THREE.Mesh(waterGeometry, waterMaterial);
        this.waterMesh.rotation.x = -Math.PI / 2;
        this.waterMesh.position.y = -2;
        this.scene.add(this.waterMesh);
    }

    createGridHelper() {
        const size = this.GRID_SIZE * this.CELL_SIZE;
        const divisions = this.GRID_SIZE;
        const gridHelper = new THREE.GridHelper(size, divisions, 0x000000, 0x444444);
        gridHelper.position.y = 0.1;
        this.scene.add(gridHelper);
    }

    // Simplex noise function for terrain generation
    noise(x, z) {
        const X = Math.floor(x) & 255;
        const Z = Math.floor(z) & 255;
        x -= Math.floor(x);
        z -= Math.floor(z);
        const u = this.fade(x);
        const w = this.fade(z);
        const A = this.p[X] + Z;
        const B = this.p[X + 1] + Z;
        return this.lerp(w,
            this.lerp(u,
                this.grad(this.p[A], x, 0, z),
                this.grad(this.p[B], x - 1, 0, z)
            ),
            this.lerp(u,
                this.grad(this.p[A + 1], x, 0, z - 1),
                this.grad(this.p[B + 1], x - 1, 0, z - 1)
            )
        );
    }

    fade(t) {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    lerp(t, a, b) {
        return a + t * (b - a);
    }

    grad(hash, x, y, z) {
        const h = hash & 15;
        const u = h < 8 ? x : y;
        const v = h < 4 ? y : h === 12 || h === 14 ? x : z;
        return ((h & 1) === 0 ? u : -u) + ((h & 2) === 0 ? v : -v);
    }

    p = new Array(512);

    initNoise() {
        const permutation = [151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,102,143,54,65,25,63,161,1,216,80,73,209,76,132,187,208,89,18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,186,3,64,52,217,226,250,124,123,5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,223,183,170,213,119,248,152,2,44,154,163,70,221,153,101,155,167,43,172,9,129,22,39,253,19,98,108,110,79,113,224,232,178,185,112,104,218,246,97,228,251,34,242,193,238,210,144,12,191,179,162,241,81,51,145,235,249,14,239,107,49,192,214,31,181,199,106,157,184,84,204,176,115,121,50,45,127,4,150,254,138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180];
        for (let i = 0; i < 256; i++) {
            this.p[i] = this.p[256 + i] = permutation[i];
        }
    }

    createGround() {
        const groundGeometry = new THREE.PlaneGeometry(
            this.GRID_SIZE * this.CELL_SIZE,
            this.GRID_SIZE * this.CELL_SIZE
        );
        const groundMaterial = new THREE.MeshStandardMaterial({
            color: 0x7ec850,
            roughness: 0.8,
        });
        this.ground = new THREE.Mesh(groundGeometry, groundMaterial);
        this.ground.rotation.x = -Math.PI / 2;
        this.ground.receiveShadow = true;
        this.ground.name = 'ground';
        this.scene.add(this.ground);
    }

    createBuildingGeometry(type, level) {
        const baseWidth = this.CELL_SIZE * 0.8;
        const baseDepth = this.CELL_SIZE * 0.8;
        let height = this.CELL_SIZE * level;

        const geometry = new THREE.BoxGeometry(baseWidth, height, baseDepth);
        const material = new THREE.MeshPhongMaterial({
            color: type.color,
            transparent: false,
            opacity: 1
        });

        const building = new THREE.Mesh(geometry, material);
        building.position.y = height / 2;
        building.castShadow = true;
        building.receiveShadow = true;

        // Add details based on building type and level
        this.addBuildingDetails(building, type, level);

        return building;
    }

    addBuildingDetails(building, type, level) {
        const baseWidth = this.CELL_SIZE * 0.8;
        const baseDepth = this.CELL_SIZE * 0.8;
        const height = this.CELL_SIZE * level;

        // Add windows
        const windowMaterial = new THREE.MeshPhongMaterial({
            color: 0xffffff,
            transparent: true,
            opacity: 0.8
        });

        // Add windows on each level
        for (let i = 0; i < level; i++) {
            const windowGeometry = new THREE.BoxGeometry(
                baseWidth * 0.2,
                baseWidth * 0.3,
                0.1
            );

            // Front windows
            const frontWindow = new THREE.Mesh(windowGeometry, windowMaterial);
            frontWindow.position.z = baseDepth / 2 + 0.1;
            frontWindow.position.y = (i + 0.5) * (height / level) - height / 2;
            building.add(frontWindow);

            // Back windows
            const backWindow = new THREE.Mesh(windowGeometry, windowMaterial);
            backWindow.position.z = -(baseDepth / 2 + 0.1);
            backWindow.position.y = (i + 0.5) * (height / level) - height / 2;
            building.add(backWindow);
        }

        // Add roof details based on type
        if (type.id === 'residential') {
            const roofGeometry = new THREE.ConeGeometry(
                baseWidth / 2,
                height * 0.2,
                4
            );
            const roofMaterial = new THREE.MeshPhongMaterial({
                color: 0x8b4513
            });
            const roof = new THREE.Mesh(roofGeometry, roofMaterial);
            roof.position.y = height / 2;
            roof.rotation.y = Math.PI / 4;
            building.add(roof);
        } else if (type.id === 'commercial') {
            const antennaGeometry = new THREE.CylinderGeometry(
                0.2,
                0.2,
                height * 0.3,
                8
            );
            const antennaMaterial = new THREE.MeshPhongMaterial({
                color: 0x888888
            });
            const antenna = new THREE.Mesh(antennaGeometry, antennaMaterial);
            antenna.position.y = height / 2;
            building.add(antenna);
        }
    }

    placeBuilding(type, position) {
        const building = this.createBuildingGeometry(type, 1);
        building.position.x = position.x;
        building.position.z = position.z;
        this.scene.add(building);

        // Store reference to the building
        this.buildings.set(position.toString(), {
            mesh: building,
            type: type,
            level: 1
        });

        return building;
    }

    upgradeBuilding(position) {
        const key = position.toString();
        const building = this.buildings.get(key);
        if (!building) return false;

        const buildingType = this.game.buildingTypes.find(t => t.id === building.type.id);
        if (!buildingType || building.level >= buildingType.maxLevel) return false;

        // Remove old building
        this.scene.remove(building.mesh);

        // Create new upgraded building
        const newLevel = building.level + 1;
        const newBuilding = this.createBuildingGeometry(buildingType, newLevel);
        newBuilding.position.x = position.x;
        newBuilding.position.z = position.z;
        this.scene.add(newBuilding);

        // Update building reference
        this.buildings.set(key, {
            mesh: newBuilding,
            type: buildingType,
            level: newLevel
        });

        return true;
    }

    showBuildingPreview(type, position) {
        if (this.buildingPreview) {
            this.scene.remove(this.buildingPreview);
        }

        const building = this.createBuildingGeometry(type, 1);
        building.position.x = position.x;
        building.position.z = position.z;
        building.material.opacity = 0.5;
        building.material.transparent = true;

        this.buildingPreview = building;
        this.scene.add(building);
    }

    hideBuildingPreview() {
        if (this.buildingPreview) {
            this.scene.remove(this.buildingPreview);
            this.buildingPreview = null;
        }
    }

    getMousePosition(event) {
        const rect = this.renderer.domElement.getBoundingClientRect();
        this.mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
        this.mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

        this.raycaster.setFromCamera(this.mouse, this.camera);
        const intersects = this.raycaster.intersectObjects([this.ground]);

        if (intersects.length > 0) {
            const point = intersects[0].point;
            return {
                x: Math.round(point.x / this.CELL_SIZE) * this.CELL_SIZE,
                z: Math.round(point.z / this.CELL_SIZE) * this.CELL_SIZE
            };
        }
        return null;
    }

    onMouseMove(event) {
        const position = this.getMousePosition(event);
        if (position && this.game.selectedBuildingType) {
            this.showBuildingPreview(this.game.selectedBuildingType, position);
        }
    }

    onMouseClick(event) {
        const position = this.getMousePosition(event);
        if (!position) return;

        // Check if there's already a building at this position
        const key = position.toString();
        const existingBuilding = this.buildings.get(key);

        if (existingBuilding) {
            // Try to upgrade the building
            const buildingId = this.game.buildings.find(b => 
                b.position.x === position.x && b.position.z === position.z
            )?.id;
            
            if (buildingId) {
                const success = this.game.upgradeBuilding(buildingId);
                if (success) {
                    this.upgradeBuilding(position);
                }
            }
        } else if (this.game.selectedBuildingType) {
            // Place new building
            this.game.addBuilding(this.game.selectedBuildingType.id, position);
            this.placeBuilding(this.game.selectedBuildingType, position);
        }
    }

    onWindowResize() {
        this.camera.aspect = window.innerWidth / window.innerHeight;
        this.camera.updateProjectionMatrix();
        this.renderer.setSize(window.innerWidth, window.innerHeight);
    }

    animate() {
        requestAnimationFrame(() => this.animate());
        
        // Animate water
        if (this.waterMesh) {
            const time = Date.now() * 0.001;
            this.waterMesh.material.opacity = 0.6 + Math.sin(time) * 0.1;
        }

        this.controls.update();
        this.renderer.render(this.scene, this.camera);
    }

    reset() {
        // Remove all buildings
        this.buildings.forEach(building => {
            this.scene.remove(building.mesh);
        });
        this.buildings.clear();
        this.hideBuildingPreview();
    }

    async createMapTerrain() {
        if (!this.mapboxToken) {
            console.error('Mapbox token not set');
            return;
        }

        try {
            // Get terrain data from Mapbox
            const terrain = await this.getTerrainData();
            if (!terrain) return;

            // Create terrain geometry
            const geometry = new THREE.PlaneGeometry(
                this.terrainSize,
                this.terrainSize,
                256,
                256
            );

            // Apply elevation data to vertices
            const vertices = geometry.attributes.position.array;
            for (let i = 0; i < vertices.length; i += 3) {
                const x = (i / 3) % 257;
                const y = Math.floor((i / 3) / 257);
                if (terrain.data[y] && terrain.data[y][x] !== undefined) {
                    vertices[i + 1] = terrain.data[y][x] * terrain.scale;
                }
            }

            geometry.computeVertexNormals();
            this.terrainGeometry = geometry;

            // Create terrain material with satellite imagery
            const material = new THREE.MeshPhongMaterial({
                map: await this.loadMapTexture(),
                shininess: 0
            });

            // Create terrain mesh
            if (this.terrain) {
                this.scene.remove(this.terrain);
            }
            this.terrain = new THREE.Mesh(geometry, material);
            this.terrain.rotation.x = -Math.PI / 2;
            this.terrain.receiveShadow = true;
            this.scene.add(this.terrain);

            // Update grid helper
            if (this.gridHelper) {
                this.scene.remove(this.gridHelper);
            }
            this.createGridHelper();

        } catch (error) {
            console.error('Error creating map terrain:', error);
            // Fallback to procedural terrain
            this.createTerrain();
        }
    }

    async getTerrainData() {
        const bbox = this.getBoundingBox();
        const url = `https://api.mapbox.com/v4/mapbox.terrain-rgb/${this.mapZoom}/${this.getTileCoordinates().x}/${this.getTileCoordinates().y}.pngraw?access_token=${this.mapboxToken}`;

        try {
            const response = await fetch(url);
            const blob = await response.blob();
            const bitmap = await createImageBitmap(blob);
            
            const canvas = document.createElement('canvas');
            canvas.width = bitmap.width;
            canvas.height = bitmap.height;
            const ctx = canvas.getContext('2d');
            ctx.drawImage(bitmap, 0, 0);
            
            const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
            const elevationData = new Array(canvas.height);
            
            for (let y = 0; y < canvas.height; y++) {
                elevationData[y] = new Array(canvas.width);
                for (let x = 0; x < canvas.width; x++) {
                    const i = (y * canvas.width + x) * 4;
                    const r = imageData.data[i];
                    const g = imageData.data[i + 1];
                    const b = imageData.data[i + 2];
                    elevationData[y][x] = -10000 + ((r * 256 * 256 + g * 256 + b) * 0.1);
                }
            }

            return {
                data: elevationData,
                scale: 0.05
            };
        } catch (error) {
            console.error('Error fetching terrain data:', error);
            return null;
        }
    }

    async loadMapTexture() {
        const url = `https://api.mapbox.com/styles/v1/mapbox/satellite-v9/static/${this.mapCenter.lng},${this.mapCenter.lat},${this.mapZoom},0,0/${1024}x${1024}?access_token=${this.mapboxToken}`;
        
        return new Promise((resolve, reject) => {
            new THREE.TextureLoader().load(
                url,
                texture => {
                    texture.minFilter = THREE.LinearFilter;
                    texture.magFilter = THREE.LinearFilter;
                    resolve(texture);
                },
                undefined,
                reject
            );
        });
    }

    getBoundingBox() {
        const R = 6378137; // Earth's radius in meters
        const zoom = this.mapZoom;
        const lat = this.mapCenter.lat;
        const lng = this.mapCenter.lng;
        
        const latRad = lat * Math.PI / 180;
        const n = Math.pow(2, zoom);
        const latDelta = this.terrainSize / (R * n);
        const lngDelta = this.terrainSize / (R * Math.cos(latRad) * n);
        
        return {
            north: lat + latDelta,
            south: lat - latDelta,
            east: lng + lngDelta,
            west: lng - lngDelta
        };
    }

    getTileCoordinates() {
        const lat = this.mapCenter.lat;
        const lng = this.mapCenter.lng;
        const zoom = this.mapZoom;
        
        const n = Math.pow(2, zoom);
        const latRad = lat * Math.PI / 180;
        const xtile = Math.floor((lng + 180) / 360 * n);
        const ytile = Math.floor((1 - Math.log(Math.tan(latRad) + 1 / Math.cos(latRad)) / Math.PI) / 2 * n);
        
        return { x: xtile, y: ytile };
    }
} 