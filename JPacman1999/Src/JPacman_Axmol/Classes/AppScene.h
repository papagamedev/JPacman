#ifndef __APPSCENE_SCENE_H__
#define __APPSCENE_SCENE_H__

#include "axmol.h"

class AppScene : public axmol::Scene
{


public:
    bool init() override;
    void update(float delta) override;

};

extern AppScene* gAppScene;


#endif  // __APPSCENE_SCENE_H__
