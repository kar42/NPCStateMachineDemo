using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SupportScripts
{
    public static class SpriteFinder
    {
        /**
         * This method will search in both directions from the given boxcollider's center position for the given distance.
         * If another collider on the Enemy layer is detected, the returned value is true. Else false.
         */
        public static bool HasNearbyCharacter(BoxCollider2D myCollider, float scoutDistance, bool ignorePlayer = false)
        {
            //This requires a raycast to hit a collider which is not own,
            //and also one of given enemy name patterns (to prevent attacking walls)
            var leftScoutDistance = myCollider.bounds.center.x - scoutDistance;
            Collider2D detectedObjectColliderLeft = ScoutAroundCollider(myCollider, leftScoutDistance, ignorePlayer);
            if (detectedObjectColliderLeft != null 
                && IsCharacterOtherThanMe(myCollider,detectedObjectColliderLeft))
            {
                //Debug.DrawLine(GetAttackPointPosition(), (Vector2)detectedObjectColliderLeft.bounds.center, Color.yellow);
                return true;
            }

            var rightScoutDistance = myCollider.bounds.center.x + scoutDistance;
            Collider2D detectedObjectColliderRight = ScoutAroundCollider(myCollider, rightScoutDistance, ignorePlayer);
            if (detectedObjectColliderRight != null 
                &&  IsCharacterOtherThanMe(myCollider,detectedObjectColliderRight))
            {
                //Debug.DrawLine(GetAttackPointPosition(), (Vector2)detectedObjectColliderRight.bounds.center, Color.blue);
                return true;
            }
            //Debug.Log(boxCollider.name+": No nearby enemy found.");
            return false;
        }
        
        /**
         * This method will search in front of the given boxcollider's center position for the given distance.
         * If another collider on the Enemy layer is detected, the returned value is true. Else false.
         */
        public static bool HasAggressiveCharacterAhead(BoxCollider2D myCollider, float scoutDistance, bool facingRight)
        {
            var characterAhead = GetCharacterAhead(myCollider, scoutDistance, facingRight);
            return characterAhead != null
                   && characterAhead.CharacterCurrentDemeanor() == Personality.EnemyPersonality.Aggressive;
        }
        
        /**
         * This method will search behind the given boxcollider's center position for the given distance.
         * If another collider on the Enemy layer is detected, the returned value is true. Else false.
         */
        public static bool HasAggressiveCharacterBehind(BoxCollider2D myCollider, float scoutDistance, bool facingRight)
        {
            var characterBehind = GetCharacterBehind(myCollider, scoutDistance, facingRight);
            return characterBehind != null
                   && characterBehind.CharacterCurrentDemeanor() == Personality.EnemyPersonality.Aggressive;
        }
        
        /**
         * This method will search in both directions from the given boxcollider's center position for the given distance.
         * If an enemy (must be different name from the given collider) is detected, their collider is returned. Else null.
         * Nearest is determined by comparing collider distance from given boxcollider.
         */
        public static Collider2D GetNearestCharacterCollider(BoxCollider2D myCollider, float scoutDistance, bool ignorePlayer = false)
        {
            Collider2D closestCollider;

            var detectedColliderLeft = ScoutAroundCollider(myCollider, myCollider.bounds.center.x - scoutDistance);
            var leftColliderHitEnemy = detectedColliderLeft != null 
                                       && IsCharacterOtherThanMe(myCollider, detectedColliderLeft)
                                       && !(ignorePlayer && IsPlayer(detectedColliderLeft.name));


            var detectedColliderRight = ScoutAroundCollider(myCollider, myCollider.bounds.center.x + scoutDistance);
            var rightColliderHitEnemy = detectedColliderRight!= null 
                                        && IsCharacterOtherThanMe(myCollider, detectedColliderRight)
                                        && !(ignorePlayer && IsPlayer(detectedColliderRight.name));

            if (leftColliderHitEnemy && rightColliderHitEnemy)
            {
                if (detectedColliderLeft.Distance(myCollider).distance <= detectedColliderRight.Distance(myCollider).distance)
                {
                    closestCollider = detectedColliderLeft;
                } 
                else
                {
                    closestCollider = detectedColliderRight;
                }
            } 
            else if (leftColliderHitEnemy)
            {
                closestCollider = detectedColliderLeft;
            }
            else if (rightColliderHitEnemy)
            {
                closestCollider = detectedColliderRight;
            }
            else
            {
                return null;

            }
            //The closest enemy has been found, now set appropriate values
            return closestCollider;

        }
        
        /**
         * This method will search in both directions from the given boxcollider's center position for the given distance.
         * If an enemy (must be different name from the given collider) is detected, their collider is returned. Else null
         */
        public static Character GetNearestCharacter(BoxCollider2D myCollider, float scoutDistance)
        {
            var nearestCharacterCollider = GetNearestCharacterCollider(myCollider, scoutDistance);
            return GetCharacterComponent(nearestCharacterCollider);
        }
        
        /**
         * This method will search in front of the given boxcollider's center position for the given distance.
         * If an enemy (must be different name from the given collider) is detected, their collider is returned. Else null
         */
        public static Character GetCharacterAhead(BoxCollider2D myCollider, float scoutDistance, bool facingRight)
        {
            var characterColliderAhead = GetCharacterColliderInDirection(myCollider, scoutDistance, facingRight);
            
            return GetCharacterComponent(characterColliderAhead);
        }
        
        /**
         * This method will search in behind the given boxcollider's center position for the given distance.
         * If an enemy (must be different name from the given collider) is detected, their collider is returned. Else null
         */
        public static Character GetCharacterBehind(BoxCollider2D myCollider, float scoutDistance, bool facingRight)
        {
            return GetCharacterAhead(myCollider, scoutDistance, !facingRight);
        }
        

        /**
         * This method will search in both directions from the given boxcollider's center position for the given distance.
         * If an enemy (must be different name from the given collider) is detected, their collider is returned. Else null.
         * Nearest is determined by comparing collider distance from given boxcollider.
         */
        public static Collider2D GetCharacterColliderInDirection(BoxCollider2D myCollider, float scoutDistance, bool facingRight)
        {
            float yPoint = myCollider.bounds.center.y;
            if (facingRight)
            {
                float rightXPoint = myCollider.bounds.center.x + myCollider.bounds.extents.x;
                float rightXScoutPoint = myCollider.bounds.center.x + myCollider.bounds.extents.x + scoutDistance;
                Collider2D detectedObjectColliderRight = ScoutForAnyCharacterCollider(
                    myCollider, new Vector2(rightXPoint,yPoint), new Vector2(rightXScoutPoint,yPoint));
                if (detectedObjectColliderRight != null 
                    &&  IsCharacterOtherThanMe(myCollider,detectedObjectColliderRight))
                {
                    //Debug.DrawLine(GetAttackPointPosition(), (Vector2)detectedObjectColliderRight.bounds.center, Color.blue);
                    return detectedObjectColliderRight;
                }
            }
            else
            {
                float leftXPoint = myCollider.bounds.center.x - myCollider.bounds.extents.x;
                float leftXScoutPoint = myCollider.bounds.center.x - myCollider.bounds.extents.x - scoutDistance;
                Collider2D detectedObjectColliderLeft = ScoutForAnyCharacterCollider(
                    myCollider, new Vector2(leftXPoint,yPoint), new Vector2(leftXScoutPoint,yPoint));
                if (detectedObjectColliderLeft != null 
                    && IsCharacterOtherThanMe(myCollider,detectedObjectColliderLeft))
                {
                    //Debug.DrawLine(GetAttackPointPosition(), (Vector2)detectedObjectColliderLeft.bounds.center, Color.yellow);
                    return detectedObjectColliderLeft;
                }
            }
            
            //Debug.Log(boxCollider.name + ": No nearby enemy returned ");
            return null;
        }

        /**
         * This method will search to the given X position.
         * If any collider other than {AttackPoints, Projectiles, NPCInteractables} is detected, their collider is returned. Else null
         */
        public static Collider2D ScoutAroundCollider(BoxCollider2D boxCollider, float scoutPointX, bool ignorePlayer = false)
        {
            var colliderFound = ScoutAroundColliderBottom(boxCollider, scoutPointX, ignorePlayer);
            if (colliderFound == null)
            {
                colliderFound = ScoutAroundColliderTop(boxCollider, scoutPointX, ignorePlayer);
            }
            return colliderFound;
        } 
        
        /**
         * This method will search to the given position from the top of the given collider.
         * If any collider other than {AttackPoints, Projectiles, NPCInteractables} is detected, their collider is returned. Else null
         */
        public static Collider2D ScoutAroundColliderTop(BoxCollider2D boxCollider, float scoutPointX, bool ignorePlayer)
        {
            //setting Y point to avoid attack colliders..?
            
            float scoutPointY = boxCollider.bounds.center.y + boxCollider.bounds.extents.y;
            var positionToScout = new Vector2(scoutPointX, scoutPointY);
            var startPosition = new Vector2(boxCollider.ClosestPoint(positionToScout).x, scoutPointY);
            return ScoutForColliders(boxCollider, startPosition, positionToScout, ignorePlayer);
        }

        /**
         * This method will search to the given position from near the bottom of the given collider.
         * If any collider other than {AttackPoints, Projectiles, NPCInteractables} is detected, their collider is returned. Else null
         */
        public static Collider2D ScoutAroundColliderBottom(BoxCollider2D boxCollider, float scoutPointX,
            bool ignorePlayer)
        {
            float scoutPointY = boxCollider.bounds.center.y - boxCollider.bounds.extents.y + 2;
            var positionToScout = new Vector2(scoutPointX, scoutPointY);
            var startPosition = new Vector2(boxCollider.ClosestPoint(positionToScout).x, scoutPointY);
            return ScoutForColliders(boxCollider, startPosition, positionToScout, ignorePlayer);
        }

        public static Collider2D[] ScoutForCharacterCollidersBetweenGivenPoints(Vector2 startPoint, Vector2 stopPoint, bool ignorePlayer)
        {
            Debug.DrawLine(startPoint, stopPoint, Color.blue);
            
                RaycastHit2D[] raycastHits = Physics2D.LinecastAll(startPoint, stopPoint);
                if (raycastHits.Length == 0)
                {
                    return null;
                }

                List<Collider2D> charactersFound = new List<Collider2D>();
                foreach (RaycastHit2D rayCastHit in raycastHits)
                {
                    Collider2D rayCastHitCollider = rayCastHit.collider;
                    if (!ShouldIgnoreCollider(rayCastHitCollider) 
                        && HasCharacterComponent(rayCastHitCollider))
                    {
                        if (!ignorePlayer 
                            && (IsPlayer(rayCastHitCollider.name) 
                                || IsPlayer(rayCastHitCollider.transform.parent.name)))
                        {
                            charactersFound.Add(rayCastHitCollider);
                        } else if (ignorePlayer 
                                   && !(IsPlayer(rayCastHitCollider.name) 
                                       || (rayCastHitCollider.transform.parent != null
                                           && IsPlayer(rayCastHitCollider.transform.parent.name))))
                        {
                            charactersFound.Add(rayCastHitCollider);
                        }
                    }
                }
                return charactersFound.ToArray();
            
        }
        
        /**
         * This method will search to the given position from the given position.
         * If any collider other than {AttackPoints, Projectiles, NPCInteractables} is detected, their collider is returned. Else null
         */
        public static Collider2D ScoutForColliders(Collider2D myCollider, Vector2 startPoint, Vector2 stopPoint, bool ignorePlayer, bool changePersonality=false)
        {
            Debug.DrawLine(startPoint, stopPoint, Color.blue);
            RaycastHit2D[] raycastHits = Physics2D.LinecastAll(startPoint, stopPoint);
            if (raycastHits.Length == 0)
            {
                return null;
            }
            foreach (RaycastHit2D rayCastHit in raycastHits)
            {
                Collider2D rayCastHitCollider = rayCastHit.collider;
                if (!ShouldIgnoreCollider(rayCastHitCollider) 
                    && !ShouldIgnoreCharacterCompletely(rayCastHitCollider, myCollider, changePersonality))
                { 
                    try
                    {
                        if (ignorePlayer 
                            && (
                                IsPlayer(rayCastHitCollider.name) 
                                || (rayCastHitCollider.transform.parent != null 
                                && IsPlayer(rayCastHitCollider.transform.parent.name))))
                        {
                            return null;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    
                    return rayCastHit.collider;
                }
            }
            return null;
        }
        
        public static Collider2D ScoutForAnyCharacterCollider(Collider2D myCollider, Vector2 startPoint, Vector2 stopPoint)
        {
            Debug.DrawLine(startPoint, stopPoint, Color.yellow);
            RaycastHit2D[] raycastHits = Physics2D.LinecastAll(startPoint, stopPoint);
            if (raycastHits.Length == 0)
            {
                return null;
            }
            foreach (RaycastHit2D rayCastHit in raycastHits)
            {
                Collider2D rayCastHitCollider = rayCastHit.collider;
                if (ShouldIgnoreCollider(rayCastHitCollider))
                {
                    continue;
                }
                if (HasCharacterComponent(rayCastHitCollider) 
                    && IsCharacterOtherThanMe(rayCastHitCollider, myCollider))
                {
                    return rayCastHit.collider;
                }
            }
            return null;
        }

        public static bool IsCharacterOtherThanMe(Collider2D myBoxCollider, Collider2D boxColliderToCheck)
        {
            if (HasCharacterComponent(boxColliderToCheck) 
                && GetCharacterName(boxColliderToCheck) != GetCharacterName(myBoxCollider))
            {
                return true;
            }
            return false;
        }
        

        public static bool HasCharacterComponent(Collider2D collider)
        {
            if (collider.GetComponent<Character>() != null)
            {
                return true;
            }
            else if (collider.GetComponentInParent<Character>() != null)
            {
                //because player moved collider to sprite level, but character is still at player level
                //also because enemy sprites have colliders at lower levels.. 
                return true;

            }
            else
            {
                return false;
            }
        }

        public static Character GetCharacterComponent(Collider2D collider)
        {
            if (collider == null)
            {
                return null;
            }
            if (collider.GetComponent<Character>() != null)
            {
                return collider.GetComponent<Character>();
            }
            else if (collider.GetComponentInParent<Character>() != null)
            {
                //because player moved collider to sprite level, but character is still at player level
                return collider.GetComponentInParent<Character>();

            }
            else
            {
                return null;
            }
        }

        public static String GetCharacterName(Collider2D collider)
        {
            if (collider == null)
            {
                return null;
            }
            if (collider.GetComponent<Character>() != null)
            {
                return collider.GetComponent<Character>().name;
            }
            else if (collider.GetComponentInParent<Character>() != null)
            {
                //because player moved collider to sprite level, but character is still at player level
                return collider.GetComponentInParent<Character>().name;

            }
            else
            {
                return null;
            }
        }
        
        /**
         * Determines whether a given x point on the graph is located ahead of a given collider's 'face'
         * which is determined as the central left or right position on the perimeter of the collider
         * dependent on the value of facingRight
         */
        public static bool IsColliderFacingXPosition(Collider2D myCollider, bool facingRight, float xPosition)
        {
            if (facingRight)
            {
                return xPosition > myCollider.bounds.center.x+myCollider.bounds.extents.x;
            }
            else
            {
                return xPosition < myCollider.bounds.center.x-myCollider.bounds.extents.x;
            }
        }
        
        /**
         * Determines whether a given Vector point on the graph for a given collider's 'face',
         * which is determined as the central left or right position on the perimeter of the collider
         * (calculated depending on the value of facingRight)
         */
        public static Vector2 GetColliderFacingCenterPosition(Collider2D collider, bool facingRight)
        {
            if (facingRight)
            {
                return new Vector2(collider.bounds.center.x+collider.bounds.extents.x, collider.bounds.center.y);
            }
            else
            {
                return new Vector2(collider.bounds.center.x-collider.bounds.extents.x, collider.bounds.center.y);
            }
        }
        /**
         * Determines whether a given Vector point on the graph for a given collider's 'face',
         * which is determined as the central left or right position on the perimeter of the collider
         * (calculated depending on the value of facingRight)
         */
        public static Vector2 GetColliderFacingFootPosition(Collider2D collider, bool facingRight)
        {
            float colliderFootPosition = collider.bounds.center.y - collider.bounds.extents.y;
            if (facingRight)
            {
                return new Vector2(collider.bounds.center.x+collider.bounds.extents.x, colliderFootPosition);
            }
            else
            {
                return new Vector2(collider.bounds.center.x-collider.bounds.extents.x, colliderFootPosition);
            }
        }
        public static Vector2 GetColliderCornerPosition(Collider2D collider, bool rightSide, bool top)
        {
            //determine either left or right X position of the collider
            float colliderXPosition = collider.bounds.center.x;
            if (rightSide)
            {
                colliderXPosition = colliderXPosition + collider.bounds.extents.x;
            }
            else
            {
                colliderXPosition = colliderXPosition - collider.bounds.extents.x;
            }

            //determine either top or bottom Y position of the collider
            float colliderYPosition = collider.bounds.center.y;
            if (top)
            {
                colliderYPosition = colliderYPosition + collider.bounds.extents.y;
            }
            else
            {
                colliderYPosition = colliderYPosition - collider.bounds.extents.y;
            }
            
            //return new vector with desired corner position
            return new Vector2(colliderXPosition, colliderYPosition);
        }

        public static bool IsAnyCharacterColliderBelowMyFeet(Collider2D collider, float distanceToCheck)
        {
            var boxCast = Physics2D.BoxCastAll(GetColliderCenterFootPosition(collider), collider.bounds.size, 
                0, Vector2.down, distanceToCheck);
            foreach (RaycastHit2D rayCastHit in boxCast){
                if (HasCharacterComponent(rayCastHit.collider) 
                    && IsCharacterOtherThanMe(collider,rayCastHit.collider)
                    && ! ShouldIgnoreCollider(rayCastHit.collider))
                {
                    if (rayCastHit.collider.IsTouching(collider) 
                        && GetColliderCenterHeadPosition(rayCastHit.collider).y <= GetColliderCenterFootPosition(collider).y)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsPlayer(string name)
        { 
            return name is "Player" or "Sprite";
        }

        public static bool IsPlayer(Collider2D colliderToCheck)
        { 
            return colliderToCheck.name == "Player" || colliderToCheck.name == "Sprite";
        }

        public static bool IsSpear(string name)
        { 
            return name == "Spear(Clone)" || name == "Spear" || name == "SpearTip";
        }

        public static bool IsSpearTip(string name)
        { 
            return name == "SpearTip";
        }
        

        public static Collider2D MaybeGetSpearTipAttached(Collider2D myCollider)
        {
            var childColliders = myCollider.GetComponentsInChildren<BoxCollider2D>();
            foreach (var collider in childColliders)
            {
                if (IsSpearTip(collider.name))
                {
                    return collider;
                }
            }
            return null;
        }

        public static bool IsMyCharacterCollider(Collider2D colliderToCheck, Collider2D myCollider)
        {
            if (colliderToCheck == myCollider)
            {
                return true;
            }

            if (HasCharacterComponent(colliderToCheck))
            {
                return GetCharacterName(colliderToCheck) == GetCharacterName(myCollider);
            }
            
            return false;
        }

        public static bool ShouldIgnoreCollider(Collider2D colliderToCheck)
        {
            int colliderHitLayer = colliderToCheck.gameObject.layer;
            return (colliderHitLayer == Constants.AttackPointLayerNum
                    || colliderHitLayer == Constants.ProjectileLayerNum
                    || colliderHitLayer == Constants.NPCInteractableLayerNum
                    || colliderHitLayer == Constants.PlayerSensorLayerNum
                    || colliderHitLayer == Constants.HitBoxLayerNum);
        }

        public static bool ShouldIgnoreSensorColliders(Collider2D colliderToCheck)
        {
            int colliderHitLayer = colliderToCheck.gameObject.layer;
            return (colliderHitLayer == Constants.AttackPointLayerNum
                    || colliderHitLayer == Constants.ProjectileLayerNum
                    || colliderHitLayer == Constants.NPCInteractableLayerNum
                    || colliderHitLayer == Constants.PlayerSensorLayerNum);
        }
        
        public static bool ShouldIgnoreCharacterCompletely(Collider2D colliderToCheck, Collider2D myCollider, bool skipNeutral=false)
        {
            if (IsMyCharacterCollider(colliderToCheck, myCollider))
            {
                return true;
            }
            if (HasCharacterComponent(colliderToCheck) 
                && IsCharacterOtherThanMe(colliderToCheck, myCollider))
            {
                var myCharacter = GetCharacterComponent(myCollider);
                if (myCharacter.IsCharacterDocileOrNeutral() && !skipNeutral)
                {
                    return true;
                }
                var collidingCharacter = GetCharacterComponent(colliderToCheck);
                if (collidingCharacter.IsCharacterDocileOrNeutral()
                    || IsSameTypeOfEnemyAsMe(colliderToCheck, myCollider))
                {
                    //todo aggressive animals should attack anything nearby?
                    return true;
                }
            }

            return false;
        }
        
    
        public static Vector2 GetColliderCenterFootPosition(Collider2D collider)
        {
            return new Vector2(collider.bounds.center.x, collider.bounds.center.y - collider.bounds.extents.y);
        }
    
        public static Vector2 GetColliderCenterHeadPosition(Collider2D collider)
        {
            return new Vector2(collider.bounds.center.x, collider.bounds.center.y + collider.bounds.extents.y);
        }

        public static bool IsEnemyOtherThanMe(string name, Collider2D myCollider)
        {
            foreach (var namePattern in Constants.EnemyNamePatterns)
            {
                MatchCollection namePatternMatch = namePattern.Matches(name);
                if (namePatternMatch.Count > 0 && name != myCollider.name)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsSameTypeOfEnemyAsMe(Collider2D colliderToCheck, Collider2D myCollider)
        {
            var name = GetCharacterName(colliderToCheck);
            var myName = GetCharacterName(myCollider);
            foreach (var namePattern in Constants.EnemyNamePatterns)
            {
                MatchCollection namePatternMatch = namePattern.Matches(name);
                MatchCollection myNamePatternMatch = namePattern.Matches(myName);
                if (namePatternMatch.Count > 0 && myNamePatternMatch.Count > 0 && name != myName)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsEnemyRunningIntoTheGround(Collider2D colliderToCheck, bool isFacingRight)
        {
            Vector2 facingFootPosition = GetColliderFacingCenterPosition(colliderToCheck, isFacingRight);
            Vector2 scanPosition;
            if (isFacingRight)
            {
                scanPosition = new Vector2(facingFootPosition.x + 20f, facingFootPosition.y);
            }
            else
            {
                scanPosition = new Vector2(facingFootPosition.x - 20f, facingFootPosition.y);
            }
            var colliderFound = ScoutForColliders(
                colliderToCheck, 
                facingFootPosition, 
                scanPosition, 
                true);
            if (colliderFound != null 
                && (colliderFound.gameObject.layer == Constants.GroundLayerNum
                || colliderFound.gameObject.layer == Constants.WallLayerNum))
            {
                return true;
            }
            
            Collider2D[] hitGroundColliders = Physics2D.OverlapCircleAll(
                facingFootPosition, 5, LayerMask.GetMask("Ground"));
            Collider2D[] hitWallColliders = Physics2D.OverlapCircleAll(
                facingFootPosition, 5, LayerMask.GetMask("Wall"));
            Collider2D[] hitPlatformColliders = Physics2D.OverlapCircleAll(
                facingFootPosition, 5, LayerMask.GetMask("Platform"));

            bool faceIsTouchingGround = hitGroundColliders.Length != 0
                                        || hitWallColliders.Length != 0;
            return faceIsTouchingGround;
        }

        public static bool IsEnemyRunningOverACliff(Collider2D colliderToCheck, bool isFacingRight)
        {
           bool isThereGroundInFrontOfMe = false;
           //get the position of the bottom front of the colliders facing direction
            Vector2 enemyFacingFootPosition = GetColliderFacingFootPosition(colliderToCheck, isFacingRight);
            
            //check for ground collider beneath enemy's facing foot position
            List<Collider2D> connectingGroundColliders = new List<Collider2D>();
            connectingGroundColliders.AddRange(Physics2D.OverlapCircleAll(
                enemyFacingFootPosition, 5, LayerMask.GetMask("Ground"))
                );
            connectingGroundColliders.AddRange(Physics2D.OverlapCircleAll(
                enemyFacingFootPosition, 5, LayerMask.GetMask("Platform"))
                );
            
            //check if corners of detected ground are near character's facing foot position
            foreach (var groundCollider in connectingGroundColliders)
            {
                Vector2 cornerPosition = GetColliderCornerPosition(groundCollider, true, true);
                var distanceToCorner = Vector2.Distance(enemyFacingFootPosition, cornerPosition);
                if (distanceToCorner < 5)
                {
                    return true;
                }
            }
            return false;
            
            //ToDo : Smarter collision detection of ground
            //- account for connected ground which is not at end of x position? Or does runningIntoGround handle that?
            // - account for ground connected to other ground.. We only care about potential falls really
        }

        public static bool IsEnemyRunningIntoAnotherCharacter(BoxCollider2D colliderToCheck, bool isFacingRight, bool ignoreDocileAndNeutral, bool ignorePlayer=false)
        {
            var faceCheckPoint = GetColliderFacingCenterPosition(colliderToCheck, isFacingRight);
            var scoutPointX = faceCheckPoint.x;
            if (isFacingRight)
            {
                faceCheckPoint.x = faceCheckPoint.x+1;
                scoutPointX = faceCheckPoint.x + 10;
            }
            else
            {
                faceCheckPoint.x = faceCheckPoint.x-1;
                scoutPointX = faceCheckPoint.x - 10;
            }
            var scoutPointY = faceCheckPoint.y;
            var positionToScout = new Vector2(scoutPointX, scoutPointY);
            var colliderInFrontOfMyFace =  ScoutForColliders(
                colliderToCheck, faceCheckPoint, positionToScout, ignorePlayer);
            if (colliderInFrontOfMyFace == null 
                || !HasCharacterComponent(colliderInFrontOfMyFace)
                || !IsCharacterOtherThanMe(colliderToCheck,colliderInFrontOfMyFace))
            {
                return false;
            }
            if (colliderInFrontOfMyFace.gameObject.layer == Constants.EnemyLayerNum
                    || colliderInFrontOfMyFace.gameObject.layer == Constants.PlayerLayerNum)
            {
                var character = GetCharacterComponent(colliderToCheck);
                if (ignoreDocileAndNeutral && character.IsCharacterDocileOrNeutral())
                {
                    return false;
                }

                return true;
            }
            return false;
        }

        public static bool IsEnemyRunningTowardsAnUnreachableTarget(Collider2D targetCollider, Collider2D myCollider)
        { 
            //this method is assuming enemy will run towards target when they are in range,
            //intended for melee attack checks
            return GetColliderCenterFootPosition(targetCollider).y >= GetColliderCenterHeadPosition(myCollider).y;
        }
        
        
    } //End SpriteFinder
    
}//End NameSpace