class Game {
    constructor(cityName) {
        this.cityName = cityName;
        this.budget = 10000;
        this.population = 0;
        this.happiness = 50;
        this.day = 1;
        this.time = "08:00";
        this.timeScale = 1;
        this.paused = false;
        this.buildings = [];
        this.selectedBuildingType = null;

        // Define building types
        this.buildingTypes = [
            {
                id: 'residential',
                name: 'Residential',
                baseCost: 1000,
                baseIncome: 50,
                basePopulation: 10,
                maxLevel: 3,
                upgradeMultiplier: 1.5,
                models: ['house', 'apartment', 'skyscraper'],
                color: 0x66bb6a
            },
            {
                id: 'commercial',
                name: 'Commercial',
                baseCost: 2000,
                baseIncome: 100,
                basePopulation: 0,
                maxLevel: 3,
                upgradeMultiplier: 1.8,
                models: ['shop', 'mall', 'megamall'],
                color: 0x42a5f5
            },
            {
                id: 'industrial',
                name: 'Industrial',
                baseCost: 3000,
                baseIncome: 200,
                basePopulation: -5,
                maxLevel: 3,
                upgradeMultiplier: 2.0,
                models: ['factory', 'warehouse', 'industrial-complex'],
                color: 0xffa726
            }
        ];

        this.cityScene = new CityScene(this);
        this.setupEventListeners();
        this.setupGameLoop();
    }

    init() {
        this.cityScene.init();
        this.updateUI();
        this.showMainMenu();
    }

    setupEventListeners() {
        // Main Menu
        document.getElementById('new-game-btn').addEventListener('click', () => this.showNewGamePanel());
        document.getElementById('load-game-btn').addEventListener('click', () => this.showLoadGamePanel());
        document.getElementById('quit-btn').addEventListener('click', () => window.close());

        // New Game Panel
        document.getElementById('start-game-btn').addEventListener('click', () => this.startNewGame());
        document.getElementById('back-to-menu-btn').addEventListener('click', () => this.showMainMenu());

        // Game Panel
        document.getElementById('pause-btn').addEventListener('click', () => this.togglePause());
        document.getElementById('speed-btn').addEventListener('click', () => this.changeSpeed());
        document.getElementById('save-btn').addEventListener('click', () => this.saveGame());
        document.getElementById('menu-btn').addEventListener('click', () => this.showMainMenu());

        // Building Buttons
        this.buildingTypes.forEach(type => {
            const button = document.createElement('button');
            button.textContent = `${type.name} ($${type.baseCost})`;
            button.addEventListener('click', () => this.selectBuildingType(type.id));
            document.getElementById('building-buttons').appendChild(button);
        });
    }

    setupGameLoop() {
        setInterval(() => {
            if (!this.paused) {
                this.update();
            }
        }, 1000 / 60);
    }

    update() {
        if (this.paused) return;

        // Update time
        const [hours, minutes] = this.time.split(':').map(Number);
        let newMinutes = minutes + 30 * this.timeScale;
        let newHours = hours;
        
        if (newMinutes >= 60) {
            newHours += Math.floor(newMinutes / 60);
            newMinutes %= 60;
            
            if (newHours >= 24) {
                newHours = 0;
                this.day++;
                this.collectIncome();
            }
        }

        this.time = `${String(newHours).padStart(2, '0')}:${String(newMinutes).padStart(2, '0')}`;
        document.getElementById('time').textContent = this.time;
        document.getElementById('day').textContent = this.day;
    }

    collectIncome() {
        const dailyIncome = this.buildings.reduce((sum, building) => sum + building.income, 0);
        this.budget += dailyIncome;
        this.updateStats();
    }

    selectBuildingType(typeId) {
        const buildingType = this.buildingTypes.find(b => b.id === typeId);
        if (buildingType && this.canAffordBuilding(buildingType)) {
            this.selectedBuildingType = buildingType;
            console.log(`Selected building type: ${buildingType.name}`);
        } else {
            console.log(`Cannot afford ${buildingType ? buildingType.name : 'building'}`);
        }
    }

    canAffordBuilding(type) {
        return this.budget >= type.baseCost;
    }

    canBuildAtHeight(type, height) {
        // Implementation needed
        return true; // Placeholder return, actual implementation needed
    }

    placeBuilding(position) {
        if (!this.selectedBuildingType || !this.canAffordBuilding(this.selectedBuildingType)) {
            return false;
        }

        // Get terrain height at position
        const height = this.cityScene.getHeightAtPosition(
            position.x * this.cityScene.CELL_SIZE,
            position.z * this.cityScene.CELL_SIZE
        );

        // Check if we can build at this height
        if (!this.canBuildAtHeight(this.selectedBuildingType, height)) {
            alert(`Cannot build ${this.selectedBuildingType.name} at this height!`);
            return false;
        }

        const building = {
            id: Date.now(),
            type: this.selectedBuildingType.id,
            position: position,
            level: 1,
            income: this.selectedBuildingType.baseIncome,
            population: this.selectedBuildingType.basePopulation,
            upgradeCost: this.selectedBuildingType.baseCost * this.selectedBuildingType.upgradeMultiplier
        };

        this.buildings.push(building);
        this.budget -= this.selectedBuildingType.baseCost;
        
        // Place building on terrain
        const buildingMesh = this.cityScene.placeBuilding(this.selectedBuildingType, position);
        if (!buildingMesh) {
            // If building couldn't be placed, refund the cost
            this.budget += this.selectedBuildingType.baseCost;
            this.buildings.pop();
            return false;
        }

        this.updateUI();
        return true;
    }

    togglePause() {
        this.paused = !this.paused;
        document.getElementById('pause-btn').textContent = this.paused ? '▶' : '⏸';
        console.log(this.paused ? 'Game paused' : 'Game resumed');
    }

    changeSpeed() {
        const speeds = [1, 2, 3];
        this.timeScale = speeds[(speeds.indexOf(this.timeScale) + 1) % speeds.length];
        document.getElementById('speed-btn').textContent = this.timeScale + 'x';
        console.log(`Game speed changed to ${this.timeScale}x`);
    }

    updateUI() {
        document.getElementById('budget').textContent = this.budget;
        document.getElementById('population').textContent = this.population;
        document.getElementById('happiness').textContent = this.happiness;
        document.getElementById('day').textContent = this.day;
        document.getElementById('time').textContent = this.time;
    }

    showMainMenu() {
        document.getElementById('main-menu').classList.remove('hidden');
        document.getElementById('new-game').classList.add('hidden');
        document.getElementById('load-game').classList.add('hidden');
        document.getElementById('game-panel').classList.add('hidden');
        this.paused = true;
        console.log('Returned to main menu');
    }

    showNewGamePanel() {
        document.getElementById('main-menu').classList.add('hidden');
        document.getElementById('new-game').classList.remove('hidden');
    }

    showLoadGamePanel() {
        document.getElementById('main-menu').classList.add('hidden');
        document.getElementById('load-game').classList.remove('hidden');
        this.loadSaveSlots();
    }

    startNewGame() {
        const cityName = document.getElementById('city-name').value || 'New City';
        this.budget = 10000;
        this.population = 0;
        this.happiness = 50;
        this.day = 1;
        this.time = "08:00";
        this.paused = false;
        this.buildings = [];
        this.cityScene.reset();
        this.updateUI();
        document.getElementById('new-game').classList.add('hidden');
        document.getElementById('game-panel').classList.remove('hidden');
    }

    async saveGame() {
        const saveData = {
            cityName: this.cityName,
            budget: this.budget,
            population: this.population,
            happiness: this.happiness,
            day: this.day,
            time: this.time,
            timeScale: this.timeScale,
            paused: this.paused,
            buildings: this.buildings,
            timestamp: new Date().toISOString()
        };

        try {
            const response = await fetch('/save', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(saveData)
            });

            if (response.ok) {
                alert('Game saved successfully!');
            } else {
                alert('Failed to save game');
            }
        } catch (error) {
            console.error('Error saving game:', error);
            alert('Error saving game');
        }
    }

    async loadSaveSlots() {
        try {
            const response = await fetch('/saves');
            const saves = await response.json();
            
            const container = document.getElementById('save-slots');
            container.innerHTML = '';

            saves.forEach(save => {
                const slot = document.createElement('div');
                slot.className = 'save-slot';
                slot.innerHTML = `
                    <h3>${save.cityName || 'Unnamed City'}</h3>
                    <p>Day ${save.day}</p>
                    <p>Population: ${save.population}</p>
                    <p>Saved: ${new Date(save.timestamp).toLocaleString()}</p>
                `;
                slot.addEventListener('click', () => this.loadGame(save));
                container.appendChild(slot);
            });
        } catch (error) {
            console.error('Error loading save slots:', error);
        }
    }

    async loadGame(saveData) {
        this.cityName = saveData.cityName;
        this.budget = saveData.budget;
        this.population = saveData.population;
        this.happiness = saveData.happiness;
        this.day = saveData.day;
        this.time = saveData.time;
        this.timeScale = saveData.timeScale;
        this.paused = saveData.paused;
        this.buildings = saveData.buildings;
        document.getElementById('load-game').classList.add('hidden');
        document.getElementById('game-panel').classList.remove('hidden');
        
        // Recreate buildings in the scene
        this.cityScene.reset();
        this.buildings.forEach(building => {
            const type = this.buildingTypes.find(t => t.id === building.type);
            if (type) {
                this.cityScene.createBuilding(type, building.position);
            }
        });
        
        this.updateUI();
    }

    updateStats() {
        this.population = this.buildings.reduce((sum, building) => sum + building.population, 0);
        const dailyIncome = this.buildings.reduce((sum, building) => sum + building.income, 0);
        
        // Update happiness based on city balance
        const residentialCount = this.buildings.filter(b => b.type === 'residential').length;
        const commercialCount = this.buildings.filter(b => b.type === 'commercial').length;
        const industrialCount = this.buildings.filter(b => b.type === 'industrial').length;
        
        this.happiness = Math.min(100, Math.max(0, 50 +
            (residentialCount > 0 ? 10 : 0) +
            (commercialCount > 0 ? 10 : 0) +
            (industrialCount > 0 ? 10 : 0) +
            (dailyIncome > 0 ? 20 : -20)
        ));

        // Update UI
        document.getElementById('budget').textContent = this.budget;
        document.getElementById('population').textContent = this.population;
        document.getElementById('happiness').textContent = this.happiness;
    }
}

// Initialize game when the page loads
window.addEventListener('load', () => {
    const game = new Game();
    game.init();
}); 