#include "AnimatedObject.hpp"
#include "mge/config.hpp"

AnimatedObject::AnimatedObject(GameObject * pObject, int pFirstFrame, int pLastFrame, float pTimeBetweenFrames,std::string pFileName, std::string pFolderName, World * pWorld)
{
	_object = pObject;
	_firstFrame = pFirstFrame;
	_lastFrame = pLastFrame;
	_timeInBetweenFrames = pTimeBetweenFrames;
	_fileName = pFileName;
	_folderName = pFolderName;
	_world = pWorld;
	_animCounter = 0;
	_litMat = dynamic_cast<LitMaterial*> (_object->getMaterial());
};

AnimatedObject::~AnimatedObject()
{
}

void AnimatedObject::Update() {

	if (_animTimer.getElapsedTime().asSeconds() > _timeInBetweenFrames) {
		_animCounter++;
		if (_animCounter == _lastFrame + 1) { 
			_animCounter = _firstFrame; 
		}
		//split the filename from lua into before and after the file extension (.png, .jpg etc)
		//so the designers can dynamically input any file they want
		vector<string> fileVector{ splitString(_fileName, '.') };
		_litMat->setTexture(_folderName + "/" + fileVector[0] + std::to_string((int)_animCounter) + '.' + fileVector[1]);
		_animTimer.restart();
	}
}

const vector<string> AnimatedObject::splitString(const string& s, const char& c)
{
	//function to split a string or array of strings
	string buff{ "" };
	vector<string> v;

	for (auto n : s)
	{
		if (n != c) buff += n; else
			if (n == c && buff != "") { v.push_back(buff); buff = ""; }
	}
	if (buff != "") v.push_back(buff);

	return v;
}