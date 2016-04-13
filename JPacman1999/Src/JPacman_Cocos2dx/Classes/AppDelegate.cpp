#include "jpacman.h"
#include "AppDelegate.h"

#include <vector>
#include <string>

#include "AppScene.h"
#include "Audio/include/SimpleAudioEngine.h"


USING_NS_CC;
using namespace std;

AppDelegate::AppDelegate() {

}

AppDelegate::~AppDelegate() 
{
}

void AppDelegate::initGLContextAttrs()
{
    GLContextAttrs glContextAttrs = {8, 8, 8, 8, 24, 8};

    GLView::setGLContextAttrs(glContextAttrs);
}

bool AppDelegate::applicationDidFinishLaunching() {
    // initialize director
    auto director = Director::getInstance();
    auto glview = director->getOpenGLView();
    if(!glview) {
        glview = GLViewImpl::create("JPacman");
        director->setOpenGLView(glview);
    }

    director->setOpenGLView(glview);

    // Set the design resolution
	glview->setFrameSize(960, 720);
    glview->setDesignResolutionSize(640, 480, ResolutionPolicy::SHOW_ALL);

	vector<string> searchPath;
	searchPath.push_back("Resources");
	FileUtils::getInstance()->setSearchPaths(searchPath);

#if _DEBUG
    // turn on display FPS
    director->setDisplayStats(true);
#endif
    // set FPS. the default value is 1.0/60 if you don't call this
    director->setAnimationInterval(1.0f / 30);

    // create a scene. it's an autorelease object
    auto scene = AppScene::scene();

    // run
    director->runWithScene(scene);

    return true;
}

// This function will be called when the app is inactive. When comes a phone call,it's be invoked too
void AppDelegate::applicationDidEnterBackground() {
    Director::getInstance()->stopAnimation();

    // if you use SimpleAudioEngine, it must be pause
	CocosDenshion::SimpleAudioEngine::getInstance()->pauseBackgroundMusic();
}

// this function will be called when the app is active again
void AppDelegate::applicationWillEnterForeground() {
    Director::getInstance()->startAnimation();

    // if you use SimpleAudioEngine, it must resume here
    CocosDenshion::SimpleAudioEngine::getInstance()->resumeBackgroundMusic();
}
