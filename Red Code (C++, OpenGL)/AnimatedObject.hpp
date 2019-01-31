#ifndef ANIMATEDOBJECT_HPP
#define ANIMATEDOBJECT_HPP

#include <stdio.h>
#include <string>
#include "mge\core\GameObject.hpp"
#include "mge\materials\AbstractMaterial.hpp"
#include "mge\materials\TextureMaterial.hpp"
#include  "SFML\System\Clock.hpp"
#include "mge\core\World.hpp"
#include "mge\materials\LitMaterial.hpp"

class AnimatedObject {
public:
	AnimatedObject(GameObject * pObject, int pFirstFrame, int pLastFrame, float pTimeBetweenFrames, std::string pFileName, std::string pFolderName, World * pWorld);
	~AnimatedObject();

	void setObject(GameObject *pObject) {
		_object = pObject;
	}

	void setFileName(std::string pName) {
		_fileName = pName;
	}

	void setFrames(int pFirst, int pLast) {
		_firstFrame = pFirst;
		_lastFrame = pLast;
	}

	void setTime(float pTime) {
		_timeInBetweenFrames = pTime;
	}

	void setFolderName(std::string pFolder) {
		_folderName = pFolder;
	}

	GameObject* getObject(){
		return _object;
	}

	void Update();			//this object is updated in LuaParser.cpp


private:
	GameObject * _object;
	World * _world;
	LitMaterial * _litMat;
	std::string _fileName;
	int _firstFrame;
	int _lastFrame;
	float _timeInBetweenFrames;
	std::string _folderName;

	sf::Clock _animTimer;
	int _animCounter;

	const vector<string> splitString(const string& s, const char& c);
};

#endif