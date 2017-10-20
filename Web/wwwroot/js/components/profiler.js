class Profiler {
	static GenerateObject(_size) {
		const newObj = {};
		for (let i = 0; i < _size; i++) {
			newObj[i] = i;
		}
		return newObj;
	}
	static GenerateArray(_size) {
		const newArr = new Array(_size);
		for (let i = 0; i < _size; i++) {
			newArr[i] = i;
		}
		return newArr;
	}
	static Profile(_iterations, ...functions) {
		class ProfilerItem {
			constructor(_name) {
				this.name = _name;
				this.slowest = 0;
				this.fastest = 0;
				this.average = 0;
				this.total = 0;
				this.countAverage = () => {
					this.average = this.total / _iterations;
				}
				this.addTime = (_time) => {
					if (this.slowest === 0 || this.slowest < _time)
						this.slowest = _time;
					if (this.fastest === 0 || this.fastest > _time)
						this.fastest = _time;
					this.total += _time;
				}
				this.toStringTime = () => {
					this.slowest = `${this.slowest}ms`;
					this.fastest = `${this.fastest}ms`;
					this.average = `${this.average}ms`;
					this.total = `${this.total}ms`;
				}
			}

		}
		const resultDict = {};
		const functionsCount = functions.length;
		for (let i = 0; i < functionsCount; i++) {
			const func = functions[i];
			if (func.name != null && func.name.length !== 0) {
				resultDict[i] = new ProfilerItem(func.name);
			} else {
				resultDict[i] = new ProfilerItem(`func_${i}`);
			}
		}

		let iteration = 0;
		let startTime = 0;
		let endTime = 0;


		while (iteration < _iterations) {
			startTime = 0;
			endTime = 0;
			if (iteration % 2 === 0) {
				for (let i = 0; i < functionsCount; i++) {
					startTime = window.performance.now();
					functions[i]();
					endTime = window.performance.now();
					resultDict[i].addTime(endTime - startTime);
				}
			} else {
				for (let i = functionsCount - 1; i >= 0; i--) {
					startTime = window.performance.now();
					functions[i]();
					endTime = window.performance.now();
					resultDict[i].addTime(endTime - startTime);
				}
			}
			iteration++;
			if (iteration === _iterations) {
				for (let i = 0; i < functionsCount; i++) {
					resultDict[i].countAverage();
					resultDict[i].toStringTime();
				}
				console.table(resultDict, ['name', 'slowest', 'fastest', 'average', 'total']);
			}
		}
	}
};