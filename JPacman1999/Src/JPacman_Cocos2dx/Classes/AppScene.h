#ifndef __APP_SCENE_H__
#define __APP_SCENE_H__

#include "cocos2d.h"

class AppScene : public cocos2d::Node
{
public:
    virtual bool init() override;
	virtual void update(float delta) override;

    static cocos2d::Scene* scene();

    // a selector callback
    void menuCloseCallback(Ref* sender);

    // implement the "static create()" method manually
    CREATE_FUNC(AppScene);
};

#endif // __HELLOWORLD_SCENE_H__
